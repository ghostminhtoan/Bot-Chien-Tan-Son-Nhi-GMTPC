using System;
using System.Text;
using BotChienTanSonNhi.Models;

namespace BotChienTanSonNhi.Services;

public static class ReceiptBuilder
{
    public static string BuildCustomerReceipt(OrderReceipt receipt)
    {
        var sb = new StringBuilder();
        sb.AppendLine("========================================");
        sb.AppendLine("       BỘT CHIÊN TÂN SƠN NHÌ");
        sb.AppendLine("    Đặc Sản Bột Chiên & Nui Chiên");
        sb.AppendLine("========================================");
        sb.AppendLine($"Hóa đơn : {receipt.OrderCode}");
        sb.AppendLine($"Thời gian: {receipt.CreatedAt:dd/MM/yyyy HH:mm:ss}");
        sb.AppendLine($"Loại đơn : {receipt.OrderType} - {receipt.PaymentMethod}");
        sb.AppendLine("----------------------------------------");
        sb.AppendLine("Tên món           SL   Đơn giá    T.Tiền");
        sb.AppendLine("----------------------------------------");

        foreach (var item in receipt.Items)
        {
            sb.AppendLine($"{item.ProductNameDisplay} ({item.EggNameDisplay})");
            if (item.Modifier.IsLessCake || item.Modifier.Extra3kCount > 0 || item.Modifier.Extra5kCount > 0)
            {
                sb.AppendLine($" + {item.ModifierDisplay}");
            }
            if (item.KitchenSummaryDisplay != "Chuẩn vị")
            {
                sb.AppendLine($" * {item.KitchenSummaryDisplay}");
            }
            sb.AppendLine($"                  {item.Quantity,2}  {item.UnitPrice,8:N0} {item.LineTotal,9:N0}đ");
        }

        sb.AppendLine("----------------------------------------");
        sb.AppendLine($"TỔNG CỘNG         : {receipt.TotalAmount,16:N0} VNĐ");
        sb.AppendLine($"Khách đưa         : {receipt.CustomerPaid,16:N0} VNĐ");
        sb.AppendLine($"Tiền thối         : {receipt.ChangeDue,16:N0} VNĐ");
        sb.AppendLine("========================================");
        sb.AppendLine("  CẢM ƠN QUÝ KHÁCH VÀ HẸN GẶP LẠI!");
        sb.AppendLine("       Hotline: 090x.xxx.xxx");
        sb.AppendLine("========================================");

        return sb.ToString();
    }

    public static string BuildKitchenTicket(OrderReceipt receipt)
    {
        var sb = new StringBuilder();
        sb.AppendLine("========================================");
        sb.AppendLine("           PHIẾU LÀM MÓN BẾP");
        sb.AppendLine($"MÃ ĐƠN: {receipt.OrderCode} | {receipt.OrderType.ToUpper()}");
        sb.AppendLine($"Giờ nhận: {receipt.CreatedAt:HH:mm:ss}");
        sb.AppendLine("========================================");

        int count = 1;
        foreach (var item in receipt.Items)
        {
            sb.AppendLine($"[{count++}] (x{item.Quantity}) {item.ProductNameDisplay} - {item.EggNameDisplay}");
            if (item.Modifier.IsLessCake || item.Modifier.Extra3kCount > 0 || item.Modifier.Extra5kCount > 0)
            {
                sb.AppendLine($"    >> {item.ModifierDisplay.ToUpper()}");
            }
            sb.AppendLine($"    ==> GHI CHÚ: [{item.KitchenSummaryDisplay}]");
            sb.AppendLine("----------------------------------------");
        }

        sb.AppendLine("========================================");
        return sb.ToString();
    }

    public static string GenerateVietQrUrl(decimal amount, string orderCode)
    {
        // VietQR template chuẩn F&B
        // Ngân hàng MBBank / Vietcombank mockup
        string bank = "MB";
        string accountNo = "090123456789";
        string template = "compact2";
        string accountName = "BOT CHIEN TAN SON NHI";
        string memo = $"TT {orderCode}";

        return $"https://img.vietqr.io/image/{bank}-{accountNo}-{template}.png?amount={(long)amount}&addInfo={Uri.EscapeDataString(memo)}&accountName={Uri.EscapeDataString(accountName)}";
    }
}

