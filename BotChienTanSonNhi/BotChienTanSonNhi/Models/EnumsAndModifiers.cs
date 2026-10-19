using System;
using System.Collections.Generic;

namespace BotChienTanSonNhi.Models;

public enum ProductKind
{
    BotChien,
    NuiChien,
    ThapCam
}

public enum EggOption
{
    MotTrung,
    HaiTrung,
    HaiTrungNhieuBanh,
    BaTrung
}

public static class EggOptionExtensions
{
    public static decimal GetBasePrice(this EggOption egg) => egg switch
    {
        EggOption.MotTrung => 22000m,
        EggOption.HaiTrung => 25000m,
        EggOption.HaiTrungNhieuBanh => 30000m,
        EggOption.BaTrung => 30000m,
        _ => 25000m
    };
}

public enum CakeTexture
{
    Vua,
    Gion,
    GionVua,
    MemVang,
    MemTrang
}

public enum ScallionOption
{
    MacDinh,
    NhieuHanh,
    KhongHanh,
    HanhSong
}

public enum PickleOption
{
    MacDinh,
    NhieuDuDu,
    DuDuRieng,
    KhongDuDu
}

public enum EggStyle
{
    ChinKy,
    GionRum,
    LongDao
}

public enum ChiliSauceOption
{
    MacDinh,
    NhieuTuongOt,
    KhongTuongOt
}

public enum FreshChiliOption
{
    BinhThuong,
    NhieuOtXay,
    ItOtXay,
    KhongOtXay
}

public class KitchenNotePreset
{
    public CakeTexture Texture { get; set; } = CakeTexture.Vua;
    public ScallionOption Scallion { get; set; } = ScallionOption.MacDinh;
    public PickleOption Pickle { get; set; } = PickleOption.MacDinh;
    public EggStyle Egg { get; set; } = EggStyle.ChinKy;
    public ChiliSauceOption ChiliSauce { get; set; } = ChiliSauceOption.MacDinh;
    public FreshChiliOption FreshChili { get; set; } = FreshChiliOption.BinhThuong;

    public KitchenNotePreset Clone()
    {
        return new KitchenNotePreset
        {
            Texture = this.Texture,
            Scallion = this.Scallion,
            Pickle = this.Pickle,
            Egg = this.Egg,
            ChiliSauce = this.ChiliSauce,
            FreshChili = this.FreshChili
        };
    }

    public string ToKitchenSummary()
    {
        var tags = new List<string>();

        if (Texture != CakeTexture.Vua)
        {
            tags.Add(Texture switch
            {
                CakeTexture.Gion => "🔥 GIÒN RỤM",
                CakeTexture.GionVua => "GIÒN VỪA",
                CakeTexture.MemVang => "MỀM VÀNG",
                CakeTexture.MemTrang => "MỀM TRẮNG",
                _ => ""
            });
        }

        if (Egg != EggStyle.ChinKy)
        {
            tags.Add(Egg == EggStyle.GionRum ? "TRỨNG GIÒN" : "🍳 LÒNG ĐÀO");
        }

        if (Scallion != ScallionOption.MacDinh)
        {
            tags.Add(Scallion switch
            {
                ScallionOption.NhieuHanh => "++HÀNH",
                ScallionOption.KhongHanh => "❌ K.HÀNH",
                ScallionOption.HanhSong => "🌿 HÀNH SỐNG",
                _ => ""
            });
        }

        if (Pickle != PickleOption.MacDinh)
        {
            tags.Add(Pickle switch
            {
                PickleOption.NhieuDuDu => "++ĐU ĐỦ",
                PickleOption.DuDuRieng => "📦 ĐU ĐỦ RIÊNG",
                PickleOption.KhongDuDu => "❌ K.ĐU ĐỦ",
                _ => ""
            });
        }

        if (ChiliSauce != ChiliSauceOption.MacDinh)
        {
            tags.Add(ChiliSauce == ChiliSauceOption.NhieuTuongOt ? "++TƯƠNG ỚT" : "❌ K.TƯƠNG");
        }

        if (FreshChili != FreshChiliOption.BinhThuong)
        {
            tags.Add(FreshChili switch
            {
                FreshChiliOption.NhieuOtXay => "🌶️🌶️ NHIỀU ỚT XAY",
                FreshChiliOption.ItOtXay => "🌶️ ÍT ỚT",
                FreshChiliOption.KhongOtXay => "❌ K.ỚT XAY",
                _ => ""
            });
        }

        return tags.Count > 0 ? string.Join(" | ", tags) : "Chuẩn vị";
    }
}

public class OrderModifier
{
    public bool IsLessCake { get; set; }
    public int Extra3kCount { get; set; }
    public int Extra5kCount { get; set; }
    public KitchenNotePreset Notes { get; set; } = new();

    public decimal CalculateModifierPrice()
    {
        decimal total = 0;
        if (IsLessCake) total -= 2000;
        total += Extra3kCount * 3000;
        total += Extra5kCount * 5000;
        return total;
    }

    public string GetModifierDisplay()
    {
        var parts = new List<string>();
        if (IsLessCake) parts.Add("Ít bánh (-2k)");
        if (Extra3kCount > 0) parts.Add($"+3k bánh (x{Extra3kCount})");
        if (Extra5kCount > 0) parts.Add($"+5k bánh (x{Extra5kCount})");
        return parts.Count > 0 ? string.Join(", ", parts) : "Bánh chuẩn";
    }

    public OrderModifier Clone()
    {
        return new OrderModifier
        {
            IsLessCake = this.IsLessCake,
            Extra3kCount = this.Extra3kCount,
            Extra5kCount = this.Extra5kCount,
            Notes = this.Notes.Clone()
        };
    }
}
