﻿using SharpDX;
using SharpDX.Direct3D11;
using System.IO;
using System.Runtime.Serialization;
using System.ComponentModel;

#if NETFX_CORE
using Windows.UI.Xaml;
namespace HelixToolkit.UWP
#else
using System.Windows;
namespace HelixToolkit.Wpf.SharpDX
#endif
{
    using System.ComponentModel;
    using Model;
    using Shaders;
    using Utilities;

    /// <summary>
    /// Implements a phong-material with its all properties
    /// Includes Diffuse, Normal, Displacement, Specular, etc. maps
    /// </summary>
    [DataContract]
    public partial class PhongMaterial : Material
    {
        /// <summary>
        /// Identifies the System.Windows.Media.Media3D.DiffuseMaterial.AmbientColor�dependency
        /// property.
        /// </summary>
        public static readonly DependencyProperty AmbientColorProperty =
            DependencyProperty.Register("AmbientColor", typeof(Color4), typeof(PhongMaterial), new PropertyMetadata((Color4)Color.Black, 
                (d, e)=> 
                {
                    ((d as Material).Core as PhongMaterialCore).AmbientColor = (Color4)e.NewValue;
                }));

        /// <summary>
        /// Identifies the System.Windows.Media.Media3D.DiffuseMaterial.Color�dependency
        /// property.
        /// </summary>
        public static readonly DependencyProperty DiffuseColorProperty =
            DependencyProperty.Register("DiffuseColor", typeof(Color4), typeof(PhongMaterial), new PropertyMetadata((Color4)Color.White,
                (d, e) =>
                {
                    ((d as Material).Core as PhongMaterialCore).DiffuseColor = (Color4)e.NewValue;
                }));

        /// <summary>
        ///         
        /// </summary>
        public static readonly DependencyProperty EmissiveColorProperty =
            DependencyProperty.Register("EmissiveColor", typeof(Color4), typeof(PhongMaterial), new PropertyMetadata((Color4)Color.Black,
                (d, e) =>
                {
                    ((d as Material).Core as PhongMaterialCore).EmissiveColor = (Color4)e.NewValue;
                }));

        /// <summary>
        ///         
        /// </summary>
        public static readonly DependencyProperty SpecularColorProperty =
            DependencyProperty.Register("SpecularColor", typeof(Color4), typeof(PhongMaterial), new PropertyMetadata((Color4)Color.Gray,
                (d, e) =>
                {
                    ((d as Material).Core as PhongMaterialCore).SpecularColor = (Color4)e.NewValue;
                }));

        /// <summary>
        ///         
        /// </summary>
        public static readonly DependencyProperty SpecularShininessProperty =
            DependencyProperty.Register("SpecularShininess", typeof(float), typeof(PhongMaterial), new PropertyMetadata(30f,
                (d, e) =>
                {
                    ((d as Material).Core as PhongMaterialCore).SpecularShininess = (float)e.NewValue;
                }));

        /// <summary>
        ///         
        /// </summary>
        public static readonly DependencyProperty ReflectiveColorProperty =
            DependencyProperty.Register("ReflectiveColor", typeof(Color4), typeof(PhongMaterial), new PropertyMetadata(new Color4(0.1f, 0.1f, 0.1f, 1.0f),
                (d, e) =>
                {
                    ((d as Material).Core as PhongMaterialCore).ReflectiveColor = (Color4)e.NewValue;
                }));

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty DiffuseMapProperty =
            DependencyProperty.Register("DiffuseMap", typeof(Stream), typeof(PhongMaterial), new PropertyMetadata(null,
                (d, e) => { ((d as Material).Core as PhongMaterialCore).DiffuseMap = e.NewValue as Stream; }));

        /// <summary>
        /// Supports alpha channel image, such as PNG.
        /// Usage: Load the image file(BMP, PNG, etc) as a stream.
        /// It can be used to replace DiffuseMap, or used as a mask and apply onto diffuse map. 
        /// The color will be cDiffuse*cAlpha.
        /// </summary>
        public static readonly DependencyProperty DiffuseAlphaMapProperty =
            DependencyProperty.Register("DiffuseAlphaMap", typeof(Stream), typeof(PhongMaterial), new PropertyMetadata(null, 
                (d,e)=> { ((d as Material).Core as PhongMaterialCore).DiffuseAlphaMap = e.NewValue as Stream; }));

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty NormalMapProperty =
            DependencyProperty.Register("NormalMap", typeof(Stream), typeof(PhongMaterial), new PropertyMetadata(null,
                (d, e) => { ((d as Material).Core as PhongMaterialCore).NormalMap = e.NewValue as Stream; }));
        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty SpecularColorMapProperty =
            DependencyProperty.Register("SpecularColorMap", typeof(Stream), typeof(PhongMaterial), new PropertyMetadata(null,
                (d, e) => { ((d as Material).Core as PhongMaterialCore).SpecularColorMap = e.NewValue as Stream; }));
        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty DisplacementMapProperty =
            DependencyProperty.Register("DisplacementMap", typeof(Stream), typeof(PhongMaterial), new PropertyMetadata(null,
                (d, e) => { ((d as Material).Core as PhongMaterialCore).DisplacementMap = e.NewValue as Stream; }));

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty DisplacementMapScaleMaskProperty =
            DependencyProperty.Register("DisplacementMapScaleMask", typeof(Vector4), typeof(PhongMaterial), new PropertyMetadata(new Vector4(0,0,0,1),
                (d, e) => { ((d as Material).Core as PhongMaterialCore).DisplacementMapScaleMask = (Vector4)e.NewValue; }));

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty DiffuseMapSamplerProperty =
            DependencyProperty.Register("DiffuseMapSampler", typeof(SamplerStateDescription), typeof(PhongMaterial), new PropertyMetadata(DefaultSamplers.LinearSamplerWrapAni4,
                (d, e) => { ((d as Material).Core as PhongMaterialCore).DiffuseMapSampler = (SamplerStateDescription)e.NewValue; }));

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty DisplacementMapSamplerProperty =
            DependencyProperty.Register("DisplacementMapSampler", typeof(SamplerStateDescription), typeof(PhongMaterial), new PropertyMetadata(DefaultSamplers.LinearSamplerWrapAni1,
                (d, e) => { ((d as Material).Core as PhongMaterialCore).DisplacementMapSampler = (SamplerStateDescription)e.NewValue; }));

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty RenderDiffuseMapProperty =
            DependencyProperty.Register("RenderDiffuseMap", typeof(bool), typeof(PhongMaterial), new PropertyMetadata(true,
                (d, e) =>
                {
                    ((d as Material).Core as PhongMaterialCore).RenderDiffuseMap = (bool)e.NewValue;
                }));
        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty RenderDiffuseAlphaMapProperty =
            DependencyProperty.Register("RenderDiffuseAlphaMap", typeof(bool), typeof(PhongMaterial), new PropertyMetadata(true,
                (d, e) =>
                {
                    ((d as Material).Core as PhongMaterialCore).RenderDiffuseAlphaMap = (bool)e.NewValue;
                }));
        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty RenderNormalMapProperty =
            DependencyProperty.Register("RenderNormalMap", typeof(bool), typeof(PhongMaterial), new PropertyMetadata(true,
                (d, e) =>
                {
                    ((d as Material).Core as PhongMaterialCore).RenderNormalMap = (bool)e.NewValue;
                }));
        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty RenderSpecularColorMapProperty =
            DependencyProperty.Register("RenderSpecularColorMap", typeof(bool), typeof(PhongMaterial), new PropertyMetadata(true,
                (d, e) =>
                {
                    ((d as Material).Core as PhongMaterialCore).RenderSpecularColorMap = (bool)e.NewValue;
                }));
        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty RenderDisplacementMapProperty =
            DependencyProperty.Register("RenderDisplacementMap", typeof(bool), typeof(PhongMaterial), new PropertyMetadata(true,
                (d, e) =>
                {
                    ((d as Material).Core as PhongMaterialCore).RenderDisplacementMap = (bool)e.NewValue;
                }));
        /// <summary>
        /// The render environment map property
        /// </summary>
        public static readonly DependencyProperty RenderEnvironmentMapProperty =
            DependencyProperty.Register("RenderEnvironmentMap", typeof(bool), typeof(PhongMaterial), new PropertyMetadata(false,
                (d, e) =>
                {
                    ((d as Material).Core as PhongMaterialCore).RenderEnvironmentMap = (bool)e.NewValue;
                }));
        /// <summary>
        /// The render shadow map property
        /// </summary>
        public static readonly DependencyProperty RenderShadowMapProperty =
            DependencyProperty.Register("RenderShadowMap", typeof(bool), typeof(PhongMaterial), new PropertyMetadata(false,
                (d, e) =>
                {
                    ((d as Material).Core as PhongMaterialCore).RenderShadowMap = (bool)e.NewValue;
                }));

        /// <summary>
        /// The enable automatic tangent
        /// </summary>
        public static readonly DependencyProperty EnableAutoTangentProperty =
            DependencyProperty.Register("EnableAutoTangent", typeof(bool), typeof(PhongMaterial), new PropertyMetadata(false,
                (d, e) =>
                {
                    ((d as Material).Core as PhongMaterialCore).EnableAutoTangent = (bool)e.NewValue;
                }));
        /// <summary>
        /// The enable tessellation property
        /// </summary>
        public static readonly DependencyProperty EnableTessellationProperty = DependencyProperty.Register("EnableTessellation", typeof(bool), typeof(PhongMaterial),
            new PropertyMetadata(false, (d, e) => { ((d as Material).Core as PhongMaterialCore).EnableTessellation = (bool)e.NewValue; }));
        /// <summary>
        /// The tessellation factor at <see cref="MaxTessellationDistance"/> property
        /// </summary>
        public static readonly DependencyProperty MaxDistanceTessellationFactorProperty =
            DependencyProperty.Register("MaxDistanceTessellationFactor", typeof(double), typeof(PhongMaterial), new PropertyMetadata(1.0, (d, e) =>
            { ((d as Material).Core as PhongMaterialCore).MaxDistanceTessellationFactor = (float)(double)e.NewValue; }));
        /// <summary>
        /// The tessellation factor at <see cref="MinTessellationDistance"/> property
        /// </summary>
        public static readonly DependencyProperty MinDistanceTessellationFactorProperty =
            DependencyProperty.Register("MinDistanceTessellationFactor", typeof(double), typeof(PhongMaterial), new PropertyMetadata(2.0, (d, e) =>
            { ((d as Material).Core as PhongMaterialCore).MinDistanceTessellationFactor = (float)(double)e.NewValue; }));
        /// <summary>
        /// The maximum tessellation distance property
        /// </summary>
        public static readonly DependencyProperty MaxTessellationDistanceProperty =
            DependencyProperty.Register("MaxTessellationDistance", typeof(double), typeof(PhongMaterial), new PropertyMetadata(50.0, (d, e) =>
            { ((d as Material).Core as PhongMaterialCore).MaxTessellationDistance = (float)(double)e.NewValue; }));
        /// <summary>
        /// The minimum tessellation distance property
        /// </summary>
        public static readonly DependencyProperty MinTessellationDistanceProperty =
            DependencyProperty.Register("MinTessellationDistance", typeof(double), typeof(PhongMaterial), new PropertyMetadata(1.0, (d, e) =>
            { ((d as Material).Core as PhongMaterialCore).MinTessellationDistance = (float)(double)e.NewValue; }));


        /// <summary>
        /// The uv transform property
        /// </summary>
        public static readonly DependencyProperty UVTransformProperty =
            DependencyProperty.Register("UVTransform", typeof(Matrix), typeof(PhongMaterial), new PropertyMetadata(Matrix.Identity, (d,e)=>
            {
                ((d as Material).Core as PhongMaterialCore).UVTransform = (Matrix)e.NewValue;
            }));


        /// <summary>
        /// Constructs a Shading Material which correspnds with 
        /// the Phong and BlinnPhong lighting models.
        /// </summary>
        public PhongMaterial() { }

        /// <summary>
        /// Gets or sets a color that represents how the material reflects System.Windows.Media.Media3D.AmbientLight.
        /// For details see: http://msdn.microsoft.com/en-us/library/windows/desktop/bb147175(v=vs.85).aspx
        /// </summary>
#if !NETFX_CORE
        [TypeConverter(typeof(Color4Converter))]
#endif
        public Color4 AmbientColor
        {
            get { return (Color4)this.GetValue(AmbientColorProperty); }
            set { this.SetValue(AmbientColorProperty, value); }
        }

        /// <summary>
        /// Gets or sets the diffuse color for the material.
        /// For details see: http://msdn.microsoft.com/en-us/library/windows/desktop/bb147175(v=vs.85).aspx
        /// </summary>
#if !NETFX_CORE
        [TypeConverter(typeof(Color4Converter))]
#endif
        public Color4 DiffuseColor
        {
            get { return (Color4)this.GetValue(DiffuseColorProperty); }
            set { this.SetValue(DiffuseColorProperty, value); }
        }

        /// <summary>
        /// Gets or sets the emissive color for the material.
        /// For details see: http://msdn.microsoft.com/en-us/library/windows/desktop/bb147175(v=vs.85).aspx
        /// </summary>
#if !NETFX_CORE
        [TypeConverter(typeof(Color4Converter))]
#endif
        public Color4 EmissiveColor
        {
            get { return (Color4)this.GetValue(EmissiveColorProperty); }
            set { this.SetValue(EmissiveColorProperty, value); }
        }

        /// <summary>
        /// A fake parameter for reflectivity of the environment map
        /// </summary>
#if !NETFX_CORE
        [TypeConverter(typeof(Color4Converter))]
#endif
        public Color4 ReflectiveColor
        {
            get { return (Color4)this.GetValue(ReflectiveColorProperty); }
            set { this.SetValue(ReflectiveColorProperty, value); }
        }

        /// <summary>
        /// Gets or sets the specular color for the material.
        /// For details see: http://msdn.microsoft.com/en-us/library/windows/desktop/bb147175(v=vs.85).aspx
        /// </summary>
#if !NETFX_CORE
        [TypeConverter(typeof(Color4Converter))]
#endif
        public Color4 SpecularColor
        {
            get { return (Color4)this.GetValue(SpecularColorProperty); }
            set { this.SetValue(SpecularColorProperty, value); }
        }

        /// <summary>
        /// The power of specular reflections. 
        /// For details see: http://msdn.microsoft.com/en-us/library/windows/desktop/bb147175(v=vs.85).aspx
        /// </summary>
        public float SpecularShininess
        {
            get { return (float)this.GetValue(SpecularShininessProperty); }
            set { this.SetValue(SpecularShininessProperty, value); }
        }

        /// <summary>
        /// System.Windows.Media.Brush to be applied as a System.Windows.Media.Media3D.Material
        /// to a 3-D model.
        /// </summary>
        public Stream DiffuseMap
        {
            get { return (Stream)this.GetValue(DiffuseMapProperty); }
            set { this.SetValue(DiffuseMapProperty, value); }
        }


        public Stream DiffuseAlphaMap
        {
            get { return (Stream)this.GetValue(DiffuseAlphaMapProperty); }
            set { this.SetValue(DiffuseAlphaMapProperty, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public Stream NormalMap
        {
            get { return (Stream)this.GetValue(NormalMapProperty); }
            set { this.SetValue(NormalMapProperty, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public Stream SpecularColorMap
        {
            get { return (Stream)this.GetValue(SpecularColorMapProperty); }
            set { this.SetValue(SpecularColorMapProperty, value); }
        }
        /// <summary>
        /// 
        /// </summary>
        public Stream DisplacementMap
        {
            get { return (Stream)this.GetValue(DisplacementMapProperty); }
            set { this.SetValue(DisplacementMapProperty, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public SamplerStateDescription DiffuseMapSampler
        {
            get { return (SamplerStateDescription)this.GetValue(DiffuseMapSamplerProperty); }
            set { this.SetValue(DiffuseMapSamplerProperty, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public SamplerStateDescription DisplacementMapSampler
        {
            get { return (SamplerStateDescription)this.GetValue(DisplacementMapSamplerProperty); }
            set { this.SetValue(DisplacementMapSamplerProperty, value); }
        }

#if !NETFX_CORE
        [TypeConverter(typeof(Vector4Converter))]
#endif
        public Vector4 DisplacementMapScaleMask
        {
            set { SetValue(DisplacementMapScaleMaskProperty, value); }
            get { return (Vector4)GetValue(DisplacementMapScaleMaskProperty); }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool RenderDiffuseMap
        {
            get { return (bool)this.GetValue(RenderDiffuseMapProperty); }
            set { this.SetValue(RenderDiffuseMapProperty, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool RenderNormalMap
        {
            get { return (bool)this.GetValue(RenderNormalMapProperty); }
            set { this.SetValue(RenderNormalMapProperty, value); }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool RenderSpecularColorMap
        {
            get { return (bool)this.GetValue(RenderSpecularColorMapProperty); }
            set { this.SetValue(RenderSpecularColorMapProperty, value); }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool RenderDiffuseAlphaMap
        {
            get { return (bool)this.GetValue(RenderDiffuseAlphaMapProperty); }
            set { this.SetValue(RenderDiffuseAlphaMapProperty, value); }
        }


        /// <summary>
        /// 
        /// </summary>
        public bool RenderDisplacementMap
        {
            get { return (bool)this.GetValue(RenderDisplacementMapProperty); }
            set { this.SetValue(RenderDisplacementMapProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [render environment map]. Default is false
        /// </summary>
        /// <value>
        ///   <c>true</c> if [render environment map]; otherwise, <c>false</c>.
        /// </value>
        public bool RenderEnvironmentMap
        {
            get { return (bool)GetValue(RenderEnvironmentMapProperty); }
            set { SetValue(RenderEnvironmentMapProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [render shadow map]. Default is false
        /// </summary>
        /// <value>
        ///   <c>true</c> if [render shadow map]; otherwise, <c>false</c>.
        /// </value>
        public bool RenderShadowMap
        {
            get { return (bool)GetValue(RenderShadowMapProperty); }
            set { SetValue(RenderShadowMapProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [enable automatic tangent].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable automatic tangent]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableAutoTangent
        {
            get { return (bool)GetValue(EnableAutoTangentProperty); }
            set { SetValue(EnableAutoTangentProperty, value); }
        }
        /// <summary>
        /// Gets or sets a value indicating whether [enable tessellation].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable tessellation]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableTessellation
        {
            set
            {
                SetValue(EnableTessellationProperty, value);
            }
            get
            {
                return (bool)GetValue(EnableTessellationProperty);
            }
        }
        /// <summary>
        /// Gets or sets the tessellation factor at <see cref="MaxTessellationDistance"/>.
        /// </summary>
        /// <value>
        /// The maximum tessellation factor.
        /// </value>
        public double MaxDistanceTessellationFactor
        {
            get { return (double)GetValue(MaxDistanceTessellationFactorProperty); }
            set { SetValue(MaxDistanceTessellationFactorProperty, value); }
        }
        /// <summary>
        /// Gets or sets the tessellation factor at <see cref="MinTessellationDistance"/>
        /// </summary>
        /// <value>
        /// The minimum tessellation factor.
        /// </value>
        public double MinDistanceTessellationFactor
        {
            get { return (double)GetValue(MinDistanceTessellationFactorProperty); }
            set { SetValue(MinDistanceTessellationFactorProperty, value); }
        }
        /// <summary>
        /// Gets or sets the maximum tessellation distance.
        /// </summary>
        /// <value>
        /// The maximum tessellation distance.
        /// </value>
        public double MaxTessellationDistance
        {
            get { return (double)GetValue(MaxTessellationDistanceProperty); }
            set { SetValue(MaxTessellationDistanceProperty, value); }
        }
        /// <summary>
        /// Gets or sets the minimum tessellation distance.
        /// </summary>
        /// <value>
        /// The minimum tessellation distance.
        /// </value>
        public double MinTessellationDistance
        {
            get { return (double)GetValue(MinTessellationDistanceProperty); }
            set { SetValue(MinTessellationDistanceProperty, value); }
        }
        /// <summary>
        /// Gets or sets the texture uv transform.
        /// </summary>
        /// <value>
        /// The uv transform.
        /// </value>
        public Matrix UVTransform
        {
            get { return (Matrix)GetValue(UVTransformProperty); }
            set { SetValue(UVTransformProperty, value); }
        }

        public PhongMaterial CloneMaterial()
        {
            return new PhongMaterial()
            {
                AmbientColor = this.AmbientColor,
                DiffuseColor = this.DiffuseColor,
                DisplacementMap = this.DisplacementMap,
                EmissiveColor = this.EmissiveColor,
                Name = this.Name,
                NormalMap = this.NormalMap,
                ReflectiveColor = this.ReflectiveColor,
                SpecularColor = this.SpecularColor,
                SpecularShininess = this.SpecularShininess,
                DiffuseMap = this.DiffuseMap,
                DiffuseAlphaMap = this.DiffuseAlphaMap,
                SpecularColorMap = this.SpecularColorMap,
                DisplacementMapScaleMask = this.DisplacementMapScaleMask,
                DiffuseMapSampler = this.DiffuseMapSampler,
                DisplacementMapSampler = this.DisplacementMapSampler,
                MaxTessellationDistance = (float)this.MaxTessellationDistance,
                MinTessellationDistance = (float)this.MinTessellationDistance,
                MaxDistanceTessellationFactor = (float)this.MaxDistanceTessellationFactor,
                MinDistanceTessellationFactor = (float)this.MinDistanceTessellationFactor,
                EnableTessellation = EnableTessellation,
                RenderDiffuseAlphaMap = RenderDiffuseAlphaMap,
                RenderDiffuseMap = RenderDiffuseMap,
                RenderDisplacementMap = RenderDisplacementMap,
                RenderNormalMap = RenderNormalMap,
                RenderEnvironmentMap = RenderEnvironmentMap,
                RenderShadowMap = RenderShadowMap,
                RenderSpecularColorMap = RenderSpecularColorMap,
                EnableAutoTangent = EnableAutoTangent,
                UVTransform = UVTransform,
            };
        }

#if !NETFX_CORE
        protected override Freezable CreateInstanceCore()
        {
            return Clone();
        }
#endif

        protected override MaterialCore OnCreateCore()
        {
            return new PhongMaterialCore()
            {
                AmbientColor = this.AmbientColor,
                DiffuseColor = this.DiffuseColor,
                DisplacementMap = this.DisplacementMap,
                EmissiveColor = this.EmissiveColor,
                Name = this.Name,
                NormalMap = this.NormalMap,
                ReflectiveColor = this.ReflectiveColor,
                SpecularColor = this.SpecularColor,
                SpecularShininess = this.SpecularShininess,
                DiffuseMap = this.DiffuseMap,
                DiffuseAlphaMap = this.DiffuseAlphaMap,
                SpecularColorMap = this.SpecularColorMap,
                DisplacementMapScaleMask = this.DisplacementMapScaleMask,
                DiffuseMapSampler = this.DiffuseMapSampler,
                DisplacementMapSampler = this.DisplacementMapSampler,
                MaxTessellationDistance = (float)this.MaxTessellationDistance,
                MinTessellationDistance = (float)this.MinTessellationDistance,
                MaxDistanceTessellationFactor = (float)this.MaxDistanceTessellationFactor,
                MinDistanceTessellationFactor = (float)this.MinDistanceTessellationFactor,
                EnableTessellation = EnableTessellation,
                RenderDiffuseAlphaMap = RenderDiffuseAlphaMap,
                RenderDiffuseMap = RenderDiffuseMap,
                RenderDisplacementMap = RenderDisplacementMap,
                RenderNormalMap = RenderNormalMap,
                RenderEnvironmentMap = RenderEnvironmentMap,
                RenderShadowMap = RenderShadowMap,
                RenderSpecularColorMap = RenderSpecularColorMap,
                EnableAutoTangent = EnableAutoTangent,
                UVTransform = UVTransform
            };
        }
    }
}
