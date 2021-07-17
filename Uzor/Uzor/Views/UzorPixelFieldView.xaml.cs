﻿/*
 
 
 */
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchTracking;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Uzor.Algorithm;

namespace Uzor.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UzorPixelFieldView : ContentView
    {
        public int LayerNumber { get; set; } = 0;
        public bool EditingMode { get; set; } = false;
        public bool MirrorMode { get; set; } = true;
        public bool GradientPreviewMode { get; set; } = false;

        private UzorData _thisData;
        public UzorData ThisData { get { return _thisData; } 
            set {
                this.WidthField = value.FieldSize;
                this.HeightField = value.FieldSize;
                this._thisData = value;
            } }

        //private bool[,] EditableField;
        //private bool[,] FieldCoreForEditing;

        public bool DeleteMode = false;
        
        public double Scale { get; set; } = 1;

        int WidthField; int HeightField;
        public UzorPixelFieldView(UzorData data)
        {
            InitializeComponent();
            this.ThisData = data;
            //this.WidthField = data.FieldSize;
            //this.HeightField = data.FieldSize;

            //EditableField = new bool[WidthField, HeightField];
           // FieldCoreForEditing = new bool[WidthField+1, HeightField+1];

            
           // Device.StartTimer(TimeSpan.FromMilliseconds(350), OnTimerTick);
        }
        public UzorPixelFieldView()
        {
            InitializeComponent();
        }
      
        /*public void SaveState()
        {
            //firstly saving current state
            this.ThisData.Layers[LayerNumber].AddNextState(EditableField);
            BasicDrawingAlgorithm.Calculate(this.ThisData.Layers[LayerNumber]);
            EditableField = this.ThisData.Layers[LayerNumber].GetLastState();
        }*/
        
       /* private bool OnTimerTick()
        {
            if (EditingMode)
                return true;

            CalculateField();
            //TODO: write new field in UzorData

            uzorFieldCanvasView.InvalidateSurface();
            return true;
        }*/

        public void DrawView()
        {
            uzorFieldCanvasView.InvalidateSurface();
        }
      
        private void OnTouchEffectAction(object sender, TouchActionEventArgs args)
        {
            if (!EditingMode)
                return;

            //float pixelSize = (float)((contentView.Width) / HeightField) * ((float)Device.Info.PixelScreenSize.Width / (float)contentView.Width);
            switch (args.Type)
            {
                case TouchActionType.Pressed:
                case TouchActionType.Moved: 
                    {
                        try
                        {
                            WritePixel(args) ;
                        }
                        catch (IndexOutOfRangeException e) { }
                       
                        
                    }
                    break;
            }  
            uzorFieldCanvasView.InvalidateSurface();
        }

        private void WritePixel(TouchActionEventArgs args) // TODO: debug clone array
        {
           // float pixelSize = (float)((contentView.Width) / HeightField) * ((float)Device.Info.PixelScreenSize.Width / (float)contentView.Width);
            float pixelSize = (float)(uzorFieldCanvasView.CanvasSize.Width / WidthField);
            var f = /*(bool[,])*/this.ThisData.Layers[LayerNumber].GetLastState();//.Clone();

            //int x = (int)(ConvertToPixel(args.Location).X / pixelSize);
            //int y = (int)(ConvertToPixel(args.Location).Y / pixelSize);

            int xLocationAfterScaling = (int)((ConvertToPixel(args.Location).X/Scale + (uzorFieldCanvasView.CanvasSize.Width - (uzorFieldCanvasView.CanvasSize.Width / Scale)) / 2));
            int yLocationAfterScaling = (int)((ConvertToPixel(args.Location).Y/Scale + (uzorFieldCanvasView.CanvasSize.Height - (uzorFieldCanvasView.CanvasSize.Height / Scale)) / 2));

            int x = (int)(xLocationAfterScaling / pixelSize);
            int y = (int)(yLocationAfterScaling / pixelSize);
            try
            {
                if (MirrorMode)
                {
                    if (x<= WidthField/2 && y<=HeightField/2)
                    {
                        f[WidthField-1-x, y] = DeleteMode ? false : true;

                        f[x, y] = DeleteMode ? false : true;
                        f[WidthField-1-x, HeightField-1- y] = DeleteMode ? false : true;
                        f[x, HeightField-1- y] = DeleteMode ? false : true;
                    }

                }
                else
                    f[x, y] = DeleteMode ? false : true;

                //this.ThisData.Layers[LayerNumber].EditLastState(f);
            }
            catch (IndexOutOfRangeException e) { }
        }
        private void onCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {

            float pixelSize = (float)uzorFieldCanvasView.CanvasSize.Width / WidthField;
            var f = this.ThisData.Layers[LayerNumber].GetLastState();
            //this.uzorFieldCanvasView.HeightRequest = contentView.Width;
            // this.uzorFieldCanvasView.WidthRequest = contentView.Width;

            SKColor backColor = ThisData.Layers[0].BackColor.ToSKColor();
            SKColor frontColor = ThisData.Layers[0].FrontColor.ToSKColor();

            SKPaint backPaint = new SKPaint { Color = backColor };
            SKPaint frontPaint = new SKPaint { Color = frontColor };

            if (GradientPreviewMode)
            {
                frontPaint.Shader = SKShader.CreateLinearGradient(
                                    new SKPoint(uzorFieldCanvasView.CanvasSize.Width / 4, uzorFieldCanvasView.CanvasSize.Height / 2),
                                    new SKPoint(uzorFieldCanvasView.CanvasSize.Width, uzorFieldCanvasView.CanvasSize.Height / 2),
                                    new SKColor[] { frontColor, new SKColor(frontColor.Red, frontColor.Green, frontColor.Blue, 0)  },
                                    new float[] { 0, 1 },
                                    SKShaderTileMode.Clamp);


                backPaint.Shader = SKShader.CreateLinearGradient(
                                    new SKPoint(uzorFieldCanvasView.CanvasSize.Width / 4, uzorFieldCanvasView.CanvasSize.Height / 2),
                                    new SKPoint(uzorFieldCanvasView.CanvasSize.Width, uzorFieldCanvasView.CanvasSize.Height / 2),
                                    new SKColor[] { backColor, new SKColor(backColor.Red, backColor.Green, backColor.Blue, 0) },
                                    new float[] { 0, 1 },
                                    SKShaderTileMode.Clamp);
            }
                  



            SKCanvas canvas = e.Surface.Canvas; 
            canvas.Clear(new SKColor(0,0,0,0));

            // zoom scene:
            canvas.Scale((float)this.Scale, (float)this.Scale, uzorFieldCanvasView.CanvasSize.Width / 2, uzorFieldCanvasView.CanvasSize.Height / 2);

            for (int w = 0; w < WidthField; w++)
                for (int h = 0; h < HeightField; h++)
                {
                    if (f[w, h] == false)
                    {
                        canvas.DrawRect((float)w * pixelSize, (float)h * pixelSize, pixelSize, pixelSize,  /*new SKPaint() { Color = backColor}*/ backPaint);
                    }
                    else
                        canvas.DrawRect((float)w * pixelSize, (float)h * pixelSize, pixelSize, pixelSize, /*new SKPaint() { Color = frontColor }*/ frontPaint);
                }



            ////////// drawing grid:
            if (EditingMode)
            {
                var paint = new SKPaint() { Color = Color.FromRgba(5, 5, 5, 20).ToSKColor(), StrokeWidth = 2 };

                for (int w = 0; w < WidthField; w++)
                    canvas.DrawLine((float)((w+1)*pixelSize), 0, (float)((w + 1) * pixelSize), (float)(uzorFieldCanvasView.CanvasSize.Height), paint);
                for (int h = 0; h < HeightField; h++)
                    canvas.DrawLine(0, (float)((h + 1) * pixelSize), (float)(uzorFieldCanvasView.CanvasSize.Width), (float)((h + 1) * pixelSize), paint);
            }
            

            
            //////////
            
           
            

            // drawing '+' in center
            if (EditingMode)
            {
                canvas.Restore();

                canvas.DrawLine((float)((uzorFieldCanvasView.CanvasSize.Width / 2.0) - 50),
                                (float)(uzorFieldCanvasView.CanvasSize.Height / 2.0),
                                (float)(uzorFieldCanvasView.CanvasSize.Width / 2.0) + 50,
                                (float)(uzorFieldCanvasView.CanvasSize.Height / 2.0),
                                new SKPaint() { Color = Color.FromRgba(10,10,10,100).ToSKColor(), StrokeWidth=10}
                                );

                canvas.DrawLine((float)((uzorFieldCanvasView.CanvasSize.Width / 2.0)), 
                                (float)(uzorFieldCanvasView.CanvasSize.Height / 2.0-50), 
                                (float)(uzorFieldCanvasView.CanvasSize.Width / 2.0),
                                (float)(uzorFieldCanvasView.CanvasSize.Height / 2.0) + 50, 
                                new SKPaint() { Color = Color.FromRgba(10, 10, 10, 100).ToSKColor(), StrokeWidth=10 }
                                );
            }

            // drawing gray indicator of !drawable field
            if (MirrorMode && EditingMode)
            {

                canvas.DrawRect((float)(uzorFieldCanvasView.CanvasSize.Width / 2.0), 0, (float)(uzorFieldCanvasView.CanvasSize.Width / 2.0),
                    uzorFieldCanvasView.CanvasSize.Width, new SKPaint() { Color = Color.FromRgba(10, 10, 10, 100).ToSKColor(), StrokeWidth = 10 });
                canvas.DrawRect(0, (float)(uzorFieldCanvasView.CanvasSize.Width / 2.0), (float)(uzorFieldCanvasView.CanvasSize.Width / 2.0),
                    (float)(uzorFieldCanvasView.CanvasSize.Width/2.0), new SKPaint() { Color = Color.FromRgba(10, 10, 10, 100).ToSKColor(), StrokeWidth = 10 });
            }
            
        }

        SKPoint ConvertToPixel(Point pt)
        {
            return new SKPoint((float)(uzorFieldCanvasView.CanvasSize.Width * pt.X / uzorFieldCanvasView.Width),
                               (float)(uzorFieldCanvasView.CanvasSize.Height * pt.Y / uzorFieldCanvasView.Height));
        }

        private void sizeChangedEvent(object sender, EventArgs e)
        {
            this.contentView.HeightRequest = contentView.Width;
        }


        /* public void SaveCurrentState(int layerNumber = 0)
         {
             this.ThisData.Layers[layerNumber].AddNextState(FieldCore);
         }*/



    }
}