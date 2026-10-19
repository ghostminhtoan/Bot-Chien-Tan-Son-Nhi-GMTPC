using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BotChienTanSonNhi.Models;
using BotChienTanSonNhi.Services;

namespace BotChienTanSonNhi.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private int _orderCounter = 1001;

    // --- Tab điều hướng ---
    [ObservableProperty]
    private int _selectedTabIndex = 0; // 0: Bán hàng, 1: Hóa đơn, 2: Thống kê ca

    // --- Cấu hình món đang chọn ---
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentUnitPrice))]
    private ProductKind _currentProduct = ProductKind.BotChien;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentUnitPrice))]
    private EggOption _currentEgg = EggOption.HaiTrung;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentUnitPrice))]
    private bool _isLessCake = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentUnitPrice))]
    private int _extra3kCount = 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentUnitPrice))]
    private int _extra5kCount = 0;

    // --- 6 Nhóm ghi chú nhanh ---
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentKitchenSummary))]
    private CakeTexture _currentTexture = CakeTexture.Vua;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentKitchenSummary))]
    private ScallionOption _currentScallion = ScallionOption.MacDinh;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentKitchenSummary))]
    private PickleOption _currentPickle = PickleOption.MacDinh;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentKitchenSummary))]
    private EggStyle _currentEggStyle = EggStyle.ChinKy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentKitchenSummary))]
    private ChiliSauceOption _currentChiliSauce = ChiliSauceOption.MacDinh;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentKitchenSummary))]
    private FreshChiliOption _currentFreshChili = FreshChiliOption.BinhThuong;

    // --- Giỏ hàng ---
    [ObservableProperty]
    private ObservableCollection<PosOrderItem> _cartItems = new();

    [ObservableProperty]
    private decimal _cartTotal = 0;

    [ObservableProperty]
    private int _cartItemCount = 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ChangeDue))]
    private decimal _customerPaid = 0;

    [ObservableProperty]
    private string _customerPaidText = "0";

    partial void OnCustomerPaidChanged(decimal value)
    {
        string formatted = value > 0 ? value.ToString("N0") : "0";
        if (_customerPaidText != formatted)
        {
            _customerPaidText = formatted;
            OnPropertyChanged(nameof(CustomerPaidText));
        }
    }

    partial void OnCustomerPaidTextChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            if (_customerPaid != 0)
            {
                _customerPaid = 0;
                OnPropertyChanged(nameof(CustomerPaid));
                OnPropertyChanged(nameof(ChangeDue));
            }
            return;
        }

        string rawDigits = new string(value.Where(char.IsDigit).ToArray());
        if (decimal.TryParse(rawDigits, out decimal parsedVal))
        {
            if (_customerPaid != parsedVal)
            {
                _customerPaid = parsedVal;
                OnPropertyChanged(nameof(CustomerPaid));
                OnPropertyChanged(nameof(ChangeDue));
            }
        }
    }

    [ObservableProperty]
    private string _orderType = "Tại chỗ"; // "Tại chỗ" hoặc "Mang về"

    // --- Modal thanh toán & Hóa đơn ---
    [ObservableProperty]
    private bool _isCheckoutModalOpen = false;

    [ObservableProperty]
    private string _currentReceiptText = string.Empty;

    [ObservableProperty]
    private string _currentKitchenTicketText = string.Empty;

    [ObservableProperty]
    private string _vietQrUrl = string.Empty;

    [ObservableProperty]
    private string _paymentModalTitle = "THANH TOÁN THÀNH CÔNG";

    // --- Lịch sử & Thống kê ---
    [ObservableProperty]
    private ObservableCollection<OrderReceipt> _completedOrders = new();

    [ObservableProperty]
    private decimal _totalShiftRevenue = 0;

    [ObservableProperty]
    private int _totalShiftOrders = 0;

    public decimal CurrentUnitPrice
    {
        get
        {
            decimal basePrice = CurrentEgg.GetBasePrice();
            if (IsLessCake) basePrice -= 2000;
            basePrice += Extra3kCount * 3000;
            basePrice += Extra5kCount * 5000;
            return basePrice;
        }
    }

    public decimal ChangeDue => Math.Max(0, CustomerPaid - CartTotal);

    public string CurrentKitchenSummary
    {
        get
        {
            var note = new KitchenNotePreset
            {
                Texture = CurrentTexture,
                Scallion = CurrentScallion,
                Pickle = CurrentPickle,
                Egg = CurrentEggStyle,
                ChiliSauce = CurrentChiliSauce,
                FreshChili = CurrentFreshChili
            };
            return note.ToKitchenSummary();
        }
    }

    public MainViewModel()
    {
        // Sample order history to start with
        AddMockOrder();
    }

    private void AddMockOrder()
    {
        var mockItem = new PosOrderItem
        {
            Product = ProductKind.BotChien,
            Egg = EggOption.HaiTrung,
            Quantity = 2,
            Modifier = new OrderModifier
            {
                IsLessCake = false,
                Extra5kCount = 1,
                Notes = new KitchenNotePreset { Texture = CakeTexture.Gion, Egg = EggStyle.GionRum }
            }
        };

        var mockReceipt = new OrderReceipt
        {
            OrderCode = "#1000",
            CreatedAt = DateTime.Now.AddMinutes(-30),
            Items = new ObservableCollection<PosOrderItem> { mockItem },
            TotalAmount = mockItem.LineTotal,
            CustomerPaid = 100000,
            ChangeDue = 40000,
            PaymentMethod = "Tiền mặt",
            OrderType = "Tại chỗ"
        };

        CompletedOrders.Add(mockReceipt);
        TotalShiftRevenue += mockReceipt.TotalAmount;
        TotalShiftOrders++;
    }

    // --- Commands Chọn Món & Trứng ---
    [RelayCommand]
    private void SelectProduct(ProductKind kind)
    {
        CurrentProduct = kind;
        OnPropertyChanged(nameof(CurrentUnitPrice));
    }

    [RelayCommand]
    private void SelectEgg(EggOption egg)
    {
        CurrentEgg = egg;
        OnPropertyChanged(nameof(CurrentUnitPrice));
    }

    // --- Commands Modifier Bánh ---
    [RelayCommand]
    private void ToggleLessCake()
    {
        IsLessCake = !IsLessCake;
        OnPropertyChanged(nameof(CurrentUnitPrice));
    }

    [RelayCommand]
    private void AddExtra3k()
    {
        Extra3kCount++;
        OnPropertyChanged(nameof(CurrentUnitPrice));
    }

    [RelayCommand]
    private void MinusExtra3k()
    {
        if (Extra3kCount > 0)
        {
            Extra3kCount--;
            OnPropertyChanged(nameof(CurrentUnitPrice));
        }
    }

    [RelayCommand]
    private void AddExtra5k()
    {
        Extra5kCount++;
        OnPropertyChanged(nameof(CurrentUnitPrice));
    }

    [RelayCommand]
    private void MinusExtra5k()
    {
        if (Extra5kCount > 0)
        {
            Extra5kCount--;
            OnPropertyChanged(nameof(CurrentUnitPrice));
        }
    }

    [RelayCommand]
    private void ResetModifiers()
    {
        IsLessCake = false;
        Extra3kCount = 0;
        Extra5kCount = 0;
        OnPropertyChanged(nameof(CurrentUnitPrice));
    }

    // --- Commands 6 Nhóm Ghi Chú ---
    [RelayCommand]
    private void SetTexture(CakeTexture texture)
    {
        CurrentTexture = texture;
        OnPropertyChanged(nameof(CurrentKitchenSummary));
    }

    [RelayCommand]
    private void SetScallion(ScallionOption scallion)
    {
        CurrentScallion = scallion;
        OnPropertyChanged(nameof(CurrentKitchenSummary));
    }

    [RelayCommand]
    private void SetPickle(PickleOption pickle)
    {
        CurrentPickle = pickle;
        OnPropertyChanged(nameof(CurrentKitchenSummary));
    }

    [RelayCommand]
    private void SetEggStyle(EggStyle style)
    {
        CurrentEggStyle = style;
        OnPropertyChanged(nameof(CurrentKitchenSummary));
    }

    [RelayCommand]
    private void SetChiliSauce(ChiliSauceOption option)
    {
        CurrentChiliSauce = option;
        OnPropertyChanged(nameof(CurrentKitchenSummary));
    }

    [RelayCommand]
    private void SetFreshChili(FreshChiliOption option)
    {
        CurrentFreshChili = option;
        OnPropertyChanged(nameof(CurrentKitchenSummary));
    }

    [RelayCommand]
    private void ResetKitchenNotes()
    {
        CurrentTexture = CakeTexture.Vua;
        CurrentScallion = ScallionOption.MacDinh;
        CurrentPickle = PickleOption.MacDinh;
        CurrentEggStyle = EggStyle.ChinKy;
        CurrentChiliSauce = ChiliSauceOption.MacDinh;
        CurrentFreshChili = FreshChiliOption.BinhThuong;
        OnPropertyChanged(nameof(CurrentKitchenSummary));
    }

    // --- Commands Quản Lý Giỏ Hàng ---
    [RelayCommand]
    private void AddToCart()
    {
        var modifier = new OrderModifier
        {
            IsLessCake = this.IsLessCake,
            Extra3kCount = this.Extra3kCount,
            Extra5kCount = this.Extra5kCount,
            Notes = new KitchenNotePreset
            {
                Texture = this.CurrentTexture,
                Scallion = this.CurrentScallion,
                Pickle = this.CurrentPickle,
                Egg = this.CurrentEggStyle,
                ChiliSauce = this.CurrentChiliSauce,
                FreshChili = this.CurrentFreshChili
            }
        };

        var newItem = new PosOrderItem
        {
            Product = this.CurrentProduct,
            Egg = this.CurrentEgg,
            Modifier = modifier,
            Quantity = 1
        };

        CartItems.Add(newItem);
        RecalculateCart();

        // Tự động gán tiền khách đưa bằng tổng tiền giỏ hàng mặc định
        CustomerPaid = CartTotal;
    }

    [RelayCommand]
    private void IncreaseQuantity(PosOrderItem item)
    {
        item.Quantity++;
        item.NotifyPriceChanged();
        RecalculateCart();
    }

    [RelayCommand]
    private void DecreaseQuantity(PosOrderItem item)
    {
        if (item.Quantity > 1)
        {
            item.Quantity--;
            item.NotifyPriceChanged();
        }
        else
        {
            CartItems.Remove(item);
        }
        RecalculateCart();
    }

    [RelayCommand]
    private void RemoveCartItem(PosOrderItem item)
    {
        CartItems.Remove(item);
        RecalculateCart();
    }

    [RelayCommand]
    private void ClearCart()
    {
        CartItems.Clear();
        RecalculateCart();
        CustomerPaid = 0;
    }

    [RelayCommand]
    private void ToggleOrderType()
    {
        OrderType = OrderType == "Tại chỗ" ? "Mang về" : "Tại chỗ";
    }

    private void RecalculateCart()
    {
        CartTotal = CartItems.Sum(x => x.LineTotal);
        CartItemCount = CartItems.Sum(x => x.Quantity);
        if (CustomerPaid < CartTotal)
        {
            CustomerPaid = CartTotal;
        }
        OnPropertyChanged(nameof(ChangeDue));
    }

    // --- Commands Numpad & Phím tiền nhanh ---
    [RelayCommand]
    private void SetExactCash()
    {
        CustomerPaid = CartTotal;
        OnPropertyChanged(nameof(ChangeDue));
    }

    [RelayCommand]
    private void AddQuickCashDelta(object? param)
    {
        decimal delta = 0;
        if (param is decimal d)
        {
            delta = d;
        }
        else if (param != null && decimal.TryParse(param.ToString(), out decimal parsed))
        {
            delta = parsed;
        }

        if (delta > 0)
        {
            CustomerPaid += delta;
            OnPropertyChanged(nameof(ChangeDue));
        }
    }

    [RelayCommand]
    private void SetQuickCash(object? param)
    {
        decimal amount = 0;
        if (param is decimal d)
        {
            amount = d;
        }
        else if (param != null && decimal.TryParse(param.ToString(), out decimal parsed))
        {
            amount = parsed;
        }

        if (amount >= 0)
        {
            CustomerPaid = amount;
            OnPropertyChanged(nameof(ChangeDue));
        }
    }

    [RelayCommand]
    private void AppendCashDigit(string digit)
    {
        string currentDigits = new string(CustomerPaidText.Where(char.IsDigit).ToArray());
        if (currentDigits == "0" || string.IsNullOrEmpty(currentDigits))
        {
            currentDigits = digit == "000" ? "0" : digit;
        }
        else
        {
            currentDigits += digit;
        }

        if (decimal.TryParse(currentDigits, out decimal val))
        {
            CustomerPaid = val;
            OnPropertyChanged(nameof(ChangeDue));
        }
    }

    [RelayCommand]
    private void BackspaceCash()
    {
        string currentDigits = new string(CustomerPaidText.Where(char.IsDigit).ToArray());
        if (currentDigits.Length > 1)
        {
            currentDigits = currentDigits.Substring(0, currentDigits.Length - 1);
        }
        else
        {
            currentDigits = "0";
        }

        if (decimal.TryParse(currentDigits, out decimal val))
        {
            CustomerPaid = val;
            OnPropertyChanged(nameof(ChangeDue));
        }
    }

    [RelayCommand]
    private void ClearCashInput()
    {
        CustomerPaid = 0;
        OnPropertyChanged(nameof(ChangeDue));
    }

    // --- Commands Thanh Toán & In Bill ---
    [RelayCommand]
    private void CheckoutCash()
    {
        if (CartItems.Count == 0) return;

        ExecuteCheckout("Tiền mặt");
    }

    [RelayCommand]
    private void CheckoutVietQr()
    {
        if (CartItems.Count == 0) return;

        CustomerPaid = CartTotal; // Với QR thanh toán chính xác
        ExecuteCheckout("Chuyển khoản VietQR");
    }

    private void ExecuteCheckout(string paymentMethod)
    {
        string orderCode = $"#{_orderCounter++}";
        var receipt = new OrderReceipt
        {
            OrderCode = orderCode,
            CreatedAt = DateTime.Now,
            Items = new ObservableCollection<PosOrderItem>(CartItems.Select(x => new PosOrderItem
            {
                Product = x.Product,
                Egg = x.Egg,
                Quantity = x.Quantity,
                Modifier = x.Modifier.Clone()
            })),
            TotalAmount = CartTotal,
            CustomerPaid = CustomerPaid,
            ChangeDue = ChangeDue,
            PaymentMethod = paymentMethod,
            OrderType = OrderType
        };

        CompletedOrders.Insert(0, receipt);
        TotalShiftRevenue += receipt.TotalAmount;
        TotalShiftOrders++;

        CurrentReceiptText = ReceiptBuilder.BuildCustomerReceipt(receipt);
        CurrentKitchenTicketText = ReceiptBuilder.BuildKitchenTicket(receipt);
        VietQrUrl = ReceiptBuilder.GenerateVietQrUrl(receipt.TotalAmount, receipt.OrderCode);
        PaymentModalTitle = $"ĐƠN HÀNG {orderCode} THÀNH CÔNG!";
        IsCheckoutModalOpen = true;

        // Reset giỏ hàng
        CartItems.Clear();
        RecalculateCart();
        CustomerPaid = 0;
    }

    [RelayCommand]
    private void CloseCheckoutModal()
    {
        IsCheckoutModalOpen = false;
    }

    [RelayCommand]
    private void SwitchTab(object? param)
    {
        if (param is int i)
        {
            SelectedTabIndex = i;
        }
        else if (param != null && int.TryParse(param.ToString(), out int parsed))
        {
            SelectedTabIndex = parsed;
        }
    }

    // --- Cập nhật Beta ---
    [RelayCommand]
    private void OpenUpdateBeta(string? platform = null)
    {
        string url = "";
        if (string.Equals(platform, "windows", StringComparison.OrdinalIgnoreCase))
        {
            url = "https://github.com/ghostminhtoan/Bot-Chien-Tan-Son-Nhi-GMTPC/releases/download/beta/BotChienTanSonNhi-GMTPC.Desktop.exe";
        }
        else if (string.Equals(platform, "linux", StringComparison.OrdinalIgnoreCase))
        {
            url = "https://github.com/ghostminhtoan/Bot-Chien-Tan-Son-Nhi-GMTPC/releases/download/beta/BotChienTanSonNhi-GMTPC-linux-x64.tar.gz";
        }
        else if (string.Equals(platform, "android", StringComparison.OrdinalIgnoreCase))
        {
            url = "https://github.com/ghostminhtoan/Bot-Chien-Tan-Son-Nhi-GMTPC/releases/download/beta/com.CompanyName.BotChienTanSonNhi-GMTPC-Signed.apk";
        }
        else if (string.Equals(platform, "release", StringComparison.OrdinalIgnoreCase))
        {
            url = "https://github.com/ghostminhtoan/Bot-Chien-Tan-Son-Nhi-GMTPC/releases/tag/beta";
        }
        else
        {
            if (OperatingSystem.IsWindows())
            {
                url = "https://github.com/ghostminhtoan/Bot-Chien-Tan-Son-Nhi-GMTPC/releases/download/beta/BotChienTanSonNhi-GMTPC.Desktop.exe";
            }
            else if (OperatingSystem.IsLinux())
            {
                url = "https://github.com/ghostminhtoan/Bot-Chien-Tan-Son-Nhi-GMTPC/releases/download/beta/BotChienTanSonNhi-GMTPC-linux-x64.tar.gz";
            }
            else if (OperatingSystem.IsAndroid())
            {
                url = "https://github.com/ghostminhtoan/Bot-Chien-Tan-Son-Nhi-GMTPC/releases/download/beta/com.CompanyName.BotChienTanSonNhi-GMTPC-Signed.apk";
            }
            else
            {
                url = "https://github.com/ghostminhtoan/Bot-Chien-Tan-Son-Nhi-GMTPC/releases/tag/beta";
            }
        }

        try
        {
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }
        catch
        {
            // ignored
        }
    }
}
