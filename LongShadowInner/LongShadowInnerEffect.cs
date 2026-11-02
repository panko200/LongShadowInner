using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.ItemEditor.CustomVisibilityAttributes;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;

namespace LongShadowInner
{
    /// <summary>
    /// 伸びる影（内側）。標準の「伸びる影」と同じパラメータを持ち、
    /// 影をテクスチャの内側に描画する。
    /// </summary>
    [VideoEffect("伸びる影（内側）", ["装飾"], ["伸びる影", "内側", "影", "ロングシャドウ", "long shadow", "flat shadow", "inner"], false, false)]
    public class LongShadowInnerEffect : VideoEffectBase
    {
        public override string Label => "伸びる影（内側）";

        [Display(GroupName = "基本", Name = "角度", Description = "影が伸びる角度(°)。")]
        [AnimationSlider("F1", "°", -360, 360)]
        public Animation Angle { get; } = new Animation(135, -100000, 100000);

        [Display(GroupName = "基本", Name = "長さ", Description = "影が伸びる長さ(px)。")]
        [AnimationSlider("F1", "px", 0, 100)]
        public Animation Length { get; } = new Animation(50, 0, 100000);

        [Display(GroupName = "基本", Name = "不透明度", Description = "影の不透明度(%)。")]
        [AnimationSlider("F1", "%", 0, 100)]
        public Animation Opacity { get; } = new Animation(100, 0, 100);

        [Display(GroupName = "基本", Name = "減衰", Description = "影の先端に向かって薄くなる度合い(%)。")]
        [AnimationSlider("F1", "%", 0, 100)]
        public Animation Attenuation { get; } = new Animation(50, 0, 100);

        [Display(GroupName = "基本", Name = "影の種類", Description = "影の塗り方。")]
        [EnumComboBox]
        public LongShadowInnerType ShadowType
        {
            get => shadowType;
            set => Set(ref shadowType, value);
        }
        private LongShadowInnerType shadowType = LongShadowInnerType.Solid;

        [Display(GroupName = "基本", Name = "色1", Description = "影の色1。単色の場合はこの色が使われます。")]
        [ColorPicker]
        [ShowPropertyEditorWhen(nameof(ShadowType), LongShadowInnerType.Solid | LongShadowInnerType.Gradient)]
        public Color Color1
        {
            get => color1;
            set => Set(ref color1, value);
        }
        private Color color1 = Colors.White;

        [Display(GroupName = "基本", Name = "色2", Description = "影の色2。グラデーションの場合は先端の色になります。")]
        [ColorPicker]
        [ShowPropertyEditorWhen(nameof(ShadowType), LongShadowInnerType.Gradient)]
        public Color Color2
        {
            get => color2;
            set => Set(ref color2, value);
        }
        private Color color2 = Colors.Black;

        public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription)
        {
            var fps = exoOutputDescription.VideoInfo.FPS;
            yield return
                $"_name=アニメーション効果\r\n" +
                $"_disable={(IsEnabled ? 0 : 1)}\r\n" +
                $"track0={Angle.ToExoString(keyFrameIndex, "F2", fps)}\r\n" +
                $"track1={Length.ToExoString(keyFrameIndex, "F2", fps)}\r\n" +
                $"track2={Opacity.ToExoString(keyFrameIndex, "F2", fps)}\r\n" +
                $"track3={Attenuation.ToExoString(keyFrameIndex, "F2", fps)}\r\n" +
                $"name=伸びる影（内側）@YMM4-未実装\r\n" +
                $"param=" +
                    $"local shadowType={(int)shadowType};" +
                    $"local color1=0x{Color1.R:X2}{Color1.G:X2}{Color1.B:X2};" +
                    $"local color2=0x{Color2.R:X2}{Color2.G:X2}{Color2.B:X2};" +
                    $"\r\n";
        }

        public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        {
            return new LongShadowInnerEffectProcessor(devices, this);
        }

        protected override IEnumerable<IAnimatable> GetAnimatables() => [Angle, Length, Opacity, Attenuation];
    }
}
