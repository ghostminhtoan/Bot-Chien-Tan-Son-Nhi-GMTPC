using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BotChienTanSonNhi.Models;

public partial class PosOrderItem : ObservableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProductNameDisplay))]
    private ProductKind _product = ProductKind.BotChien;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UnitPrice))]
    [NotifyPropertyChangedFor(nameof(LineTotal))]
    [NotifyPropertyChangedFor(nameof(EggNameDisplay))]
    private EggOption _egg = EggOption.HaiTrung;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UnitPrice))]
    [NotifyPropertyChangedFor(nameof(LineTotal))]
    [NotifyPropertyChangedFor(nameof(ModifierDisplay))]
    [NotifyPropertyChangedFor(nameof(KitchenSummaryDisplay))]
    private OrderModifier _modifier = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LineTotal))]
    private int _quantity = 1;

    public decimal UnitPrice => Egg.GetBasePrice() + Modifier.CalculateModifierPrice();
    public decimal LineTotal => UnitPrice * Quantity;

    public string ProductNameDisplay => Product switch
    {
        ProductKind.BotChien => "BỘT CHIÊN",
        ProductKind.NuiChien => "NUI CHIÊN",
        ProductKind.ThapCam => "THẬP CẨM",
        _ => "BỘT CHIÊN"
    };

    public string EggNameDisplay => Egg switch
    {
        EggOption.MotTrung => "1 Trứng (22k)",
        EggOption.HaiTrung => "2 Trứng (25k)",
        EggOption.HaiTrungNhieuBanh => "2 Trứng nhiều bánh (30k)",
        EggOption.BaTrung => "3 Trứng (30k)",
        _ => "2 Trứng"
    };

    public string ModifierDisplay => Modifier.GetModifierDisplay();
    public string KitchenSummaryDisplay => Modifier.Notes.ToKitchenSummary();

    public void NotifyPriceChanged()
    {
        OnPropertyChanged(nameof(UnitPrice));
        OnPropertyChanged(nameof(LineTotal));
        OnPropertyChanged(nameof(ModifierDisplay));
        OnPropertyChanged(nameof(KitchenSummaryDisplay));
    }
}

public class OrderReceipt
{
    public string OrderCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public ObservableCollection<PosOrderItem> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public decimal CustomerPaid { get; set; }
    public decimal ChangeDue { get; set; }
    public string PaymentMethod { get; set; } = "Tiền mặt";
    public string OrderType { get; set; } = "Tại chỗ";
}
