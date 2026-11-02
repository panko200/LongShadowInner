using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Vortice;
using Vortice.Direct2D1;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;

namespace LongShadowInner
{
    public class LongShadowInnerCustomEffect : D2D1CustomShaderEffectBase
    {
        public LongShadowInnerCustomEffect(IGraphicsDevicesAndContext devices) : base(Create<EffectImpl>(devices)) { }

        public float Angle { set => SetValue((int)EffectImpl.Props.Angle, value); }
        public float Length { set => SetValue((int)EffectImpl.Props.Length, value); }
        public float Opacity { set => SetValue((int)EffectImpl.Props.Opacity, value); }
        public float Attenuation { set => SetValue((int)EffectImpl.Props.Attenuation, value); }
        public Vector4 Color1 { set => SetValue((int)EffectImpl.Props.Color1, value); }
        public Vector4 Color2 { set => SetValue((int)EffectImpl.Props.Color2, value); }

        [CustomEffect(1)]
        internal class EffectImpl : D2D1CustomShaderEffectImplBase<EffectImpl>
        {
            [StructLayout(LayoutKind.Sequential)]
            struct ConstantBuffer
            {
                public float Angle;
                public float Length;
                public float Opacity;
                public float Attenuation;
                public Vector4 Color1;
                public Vector4 Color2;
            }

            public enum Props { Angle, Length, Opacity, Attenuation, Color1, Color2 }

            private ConstantBuffer constants;

            public EffectImpl() : base(ShaderResourceLoader.GetShaderResource("LongShadowInner.cso"))
            {
                constants = new ConstantBuffer
                {
                    Angle = 135f * MathF.PI / 180f,
                    Length = 50f,
                    Opacity = 1f,
                    Attenuation = 0.5f,
                    Color1 = Vector4.One,
                    Color2 = Vector4.UnitW,
                };
            }

            protected override void UpdateConstants()
            {
                drawInformation?.SetPixelShaderConstantBuffer(constants);
            }

            [CustomEffectProperty(PropertyType.Float, (int)Props.Angle)]
            public float Angle { get => constants.Angle; set { constants.Angle = value; UpdateConstants(); } }

            [CustomEffectProperty(PropertyType.Float, (int)Props.Length)]
            public float Length { get => constants.Length; set { constants.Length = Math.Max(value, 0f); UpdateConstants(); } }

            [CustomEffectProperty(PropertyType.Float, (int)Props.Opacity)]
            public float Opacity { get => constants.Opacity; set { constants.Opacity = Math.Clamp(value, 0f, 1f); UpdateConstants(); } }

            [CustomEffectProperty(PropertyType.Float, (int)Props.Attenuation)]
            public float Attenuation { get => constants.Attenuation; set { constants.Attenuation = Math.Clamp(value, 0f, 1f); UpdateConstants(); } }

            [CustomEffectProperty(PropertyType.Vector4, (int)Props.Color1)]
            public Vector4 Color1 { get => constants.Color1; set { constants.Color1 = value; UpdateConstants(); } }

            [CustomEffectProperty(PropertyType.Vector4, (int)Props.Color2)]
            public Vector4 Color2 { get => constants.Color2; set { constants.Color2 = value; UpdateConstants(); } }

            /// <summary>
            /// 内側に描くだけなので出力範囲は入力と同じ。
            /// </summary>
            public override void MapInputRectsToOutputRect(RawRect[] inputRects, RawRect[] inputOpaqueSubRects, out RawRect outputRect, out RawRect outputOpaqueSubRect)
            {
                inputRect = inputRects[0];
                outputRect = inputRect;
                outputOpaqueSubRect = default;
            }

            /// <summary>
            /// 各画素は進行方向へ長さぶんサンプリングするので、その範囲を入力として要求する。
            /// </summary>
            public override void MapOutputRectToInputRects(RawRect outputRect, RawRect[] inputRects)
            {
                var vector =
                    Vector2.Transform(
                        new Vector2(0, Length),
                        Matrix3x2.CreateRotation(Angle));

                var sampleEndRect =
                    new RawRect(
                        outputRect.Left + (int)MathF.Floor(vector.X),
                        outputRect.Top + (int)MathF.Floor(vector.Y),
                        outputRect.Right + (int)MathF.Ceiling(vector.X),
                        outputRect.Bottom + (int)MathF.Ceiling(vector.Y));

                inputRects[0] = new RawRect(
                    Math.Min(outputRect.Left, sampleEndRect.Left),
                    Math.Min(outputRect.Top, sampleEndRect.Top),
                    Math.Max(outputRect.Right, sampleEndRect.Right),
                    Math.Max(outputRect.Bottom, sampleEndRect.Bottom));
            }
        }
    }
}
