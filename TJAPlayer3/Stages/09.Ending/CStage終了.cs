﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using SharpDX.Direct3D9;
using FDK;
using SharpDX;
using System.Drawing;

namespace TJAPlayer3
{
	internal class CStage終了 : CStage
	{
		// コンストラクタ

		public CStage終了()
		{
			base.eステージID = CStage.Eステージ.終了;
			base.eフェーズID = CStage.Eフェーズ.共通_通常状態;
			base.b活性化してない = true;
		}


		// CStage 実装

		public override void On活性化()
		{
			this.voice終了画面 = new CSkin.Cシステムサウンド(@"Sounds\EndVoice.ogg", false, false, ESoundGroup.Voice);
			Trace.TraceInformation( "終了ステージを活性化します。" );
			Trace.Indent();
			try
			{
				this.ctAnimation = new CCounter();
				Discord.UpdatePresence("", "Ending", TJAPlayer3.StartupTime);
				base.On活性化();
			}
			finally
			{
				Trace.TraceInformation( "終了ステージの活性化を完了しました。" );
				Trace.Unindent();
			}
		}
		public override void On非活性化()
		{
			Trace.TraceInformation( "終了ステージを非活性化します。" );
			Trace.Indent();
			try
			{
				base.On非活性化();
			}
			finally
			{
				Trace.TraceInformation( "終了ステージの非活性化を完了しました。" );
				Trace.Unindent();
			}
		}
		public override void OnManagedリソースの作成()
		{
			if( !base.b活性化してない )
			{
				base.OnManagedリソースの作成();
			}
		}
		public override void OnManagedリソースの解放()
		{
			if( !base.b活性化してない )
			{
				TJAPlayer3.t安全にDisposeする(ref this.BackSurface);
				base.OnManagedリソースの解放();
			}
		}
		public override int On進行描画()
		{
			if( !TJAPlayer3.ConfigIni.bEndingAnime || TJAPlayer3.Input管理.Keyboard.bキーが押された((int)SlimDXKeys.Key.Escape)) //2017.01.27 DD
			{
				return 1;
			}
			if( !base.b活性化してない )
			{
				if( base.b初めての進行描画 )
				{
					TJAPlayer3.Skin.soundゲーム終了音.t再生する();
					this.voice終了画面.t再生する();
					this.ctAnimation.t開始( 0, 1000, 1, TJAPlayer3.Timer );
					base.b初めての進行描画 = false;
				}
				this.ctAnimation.t進行();
				if (TJAPlayer3.Tx.Exit_Background != null)
				{
					TJAPlayer3.Tx.Exit_Background.t2D描画(TJAPlayer3.app.Device, 0, 0);
				}
				/*	if (this.BackSurface != null)
					this.BackSurface.t2D描画(TJAPlayer3.app.Device, 0, 0);

                #region[幕のアニメーションの計算]
                double t = this.ctAnimation.n現在の値, c = -1300, b = 1300, d = 3000;
				int x;
				t = t / d - 1;
				x =(int)( -c * (Math.Pow(t, 4) - 1) + b);
                #endregion

                if (TJAPlayer3.Tx.Exit_Curtain != null && TJAPlayer3.Tx.Exit_Text != null)
				{
					const double n = 1500.0;
					double t2 = Math.Min(Math.Max(this.ctAnimation.n現在の値 - 1000, 0), n), c2 = 1000, b2 = -1000, d2 = n;
					int y;
					t2 = t2 / d2;
					if (t2 < 1.0 / 2.75)
					{
						y = (int)(c2 * (7.5625 * t2 * t2) + b2);
					}
					else if (t2 < 2.0 / 2.75)
					{
						t2 = t2 - (1.5 / 2.75);
						y = (int)((c2 * (7.5625 * t2 * t2 + 0.75) + b2) * 0.5);
					}
					else if (t2 < 2.5 / 2.75)
					{
						t2 = t2 - (2.25 / 2.75);
						y = (int)((c2 * (7.5625 * t2 * t2 + 0.9375) + b2) * 0.5);
					}
					else
					{
						t2 = t2 - (2.625 / 2.75);
						y = (int)((c2 * (7.5625 * t2 * t2 + 0.984375) + b2) * 0.5);
					}

					TJAPlayer3.Tx.Exit_Curtain.t2D描画(TJAPlayer3.app.Device, x, 0);

					TJAPlayer3.Tx.Exit_Text.t2D描画(TJAPlayer3.app.Device, 0, y);

				}
				else if(TJAPlayer3.Tx.Exit_Background!=null)
					TJAPlayer3.Tx.Exit_Background.t2D描画(TJAPlayer3.app.Device, x, 0);*/

				if (this.ctAnimation.b終了値に達した && !TJAPlayer3.Skin.soundゲーム終了音.b再生中)
				{
					return 1;
				}
			}
			return 0;
		}

		public void MakeBackTexture() {
			TJAPlayer3.t安全にDisposeする(ref this.BackSurface);
			using (Surface pSurface = TJAPlayer3.app.Device.GetRenderTarget(0))
			{
				using (DataStream datastream = Surface.ToStream(pSurface, ImageFileFormat.Bmp))
				{
					this.BackSurface = TJAPlayer3.tテクスチャの生成(new Bitmap(datastream));
				}
			}
		}


		// その他

		#region [ private ]
		//-----------------
		private CCounter ctAnimation;
		private CTexture BackSurface;
        private CSkin.Cシステムサウンド voice終了画面;
        //-----------------
        #endregion
    }
}
