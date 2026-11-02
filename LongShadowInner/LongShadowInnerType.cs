using System.ComponentModel.DataAnnotations;

namespace LongShadowInner
{
    /// <summary>影の種類。標準の「伸びる影」と同じ並び。</summary>
    public enum LongShadowInnerType
    {
        [Display(Name = "画像", Description = "図形の色で影を描画します。")]
        Image = 1,

        [Display(Name = "単色", Description = "色1の単色で影を描画します。")]
        Solid = 2,

        [Display(Name = "グラデーション", Description = "色1から色2へのグラデーションで影を描画します。")]
        Gradient = 4,
    }
}
