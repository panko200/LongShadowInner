using System;
using System.Numerics;
using System.Windows.Media;
using Vortice.Direct2D1;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Player.Video.Effects;

namespace LongShadowInner
{
    /// <summary>
    /// 標準の「伸びる影」と同じくエフェクトチェーンに組み込む方式のプロセッサ。
    /// </summary>
    public class LongShadowInnerEffectProcessor : VideoEffectProcessorBase
    {
        readonly LongShadowInnerEffect item;
        LongShadowInnerCustomEffect? effect;
        bool isFirst = true;
        double angle,
               shadowLength,
               opacity,
               attenuation;
        Color color1;
        Color color2;

        public LongShadowInnerEffectProcessor(IGraphicsDevicesAndContext devices, LongShadowInnerEffect item) : base(devices)
        {
            this.item = item;
        }

        public override DrawDescription Update(EffectDescription effectDescription)
        {
            if (IsPassThroughEffect || effect is null)
                return effectDescription.DrawDescription;

            var frame = effectDescription.ItemPosition.Frame;
            var length = effectDescription.ItemDuration.Frame;
            var fps = effectDescription.FPS;

            var angle = item.Angle.GetValue(frame, length, fps);
            var shadowLength = item.Length.GetValue(frame, length, fps);
            var opacity = item.Opacity.GetValue(frame, length, fps);
            var attenuation = item.Attenuation.GetValue(frame, length, fps);
            var color1 = item.ShadowType is LongShadowInnerType.Image ? Colors.Transparent : item.Color1;
            var color2 = item.ShadowType switch
            {
                LongShadowInnerType.Image => Colors.Transparent,
                LongShadowInnerType.Solid => item.Color1,
                LongShadowInnerType.Gradient => item.Color2,
                _ => Colors.Transparent,
            };

            if (isFirst || this.angle != angle || this.shadowLength != shadowLength || this.opacity != opacity || this.attenuation != attenuation || this.color1 != color1 || this.color2 != color2)
            {
                effect.Angle = (float)angle / 180 * MathF.PI;
                effect.Length = (float)shadowLength;
                effect.Opacity = (float)opacity / 100;
                effect.Attenuation = (float)attenuation / 100;
                effect.Color1 = ToPremultiplied(color1);
                effect.Color2 = ToPremultiplied(color2);
            }

            isFirst = false;
            this.angle = angle;
            this.shadowLength = shadowLength;
            this.opacity = opacity;
            this.attenuation = attenuation;
            this.color1 = color1;
            this.color2 = color2;

            return effectDescription.DrawDescription;
        }

        static Vector4 ToPremultiplied(Color color)
            => new(color.R / 255f * color.A / 255f, color.G / 255f * color.A / 255f, color.B / 255f * color.A / 255f, color.A / 255f);

        protected override void ClearEffectChain()
        {
            effect?.SetInput(0, null, true);
        }

        protected override ID2D1Image? CreateEffect(IGraphicsDevicesAndContext devices)
        {
            effect = new LongShadowInnerCustomEffect(devices);
            if (!effect.IsEnabled)
            {
                effect.Dispose();
                effect = null;
                return null;
            }
            disposer.Collect(effect);

            var output = effect.Output;
            disposer.Collect(output);
            return output;
        }

        protected override void setInput(ID2D1Image? input)
        {
            effect?.SetInput(0, input, true);
        }
    }
}
