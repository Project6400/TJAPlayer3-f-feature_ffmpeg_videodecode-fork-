﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using FDK;

namespace TJAPlayer3
{
	internal class CAct演奏判定文字列共通 : CActivity
	{
		// プロパティ

		protected STSTATUS[] st状態 = new STSTATUS[ 12 ];
		[StructLayout( LayoutKind.Sequential )]
		protected struct STSTATUS
		{
			public bool b使用中;
			public CCounter ct進行;
			public E判定 judge;
			public int n相対X座標;
			public int n相対Y座標;
			public int n透明度;
			public int nPlayer;                             // 2017.08.15 kairera0467
		}

		protected readonly ST判定文字列[] st判定文字列;
		[StructLayout(LayoutKind.Sequential)]
		protected struct ST判定文字列
		{
			public Rectangle rc;
		}
		
		//protected CTexture tx判定文字列;

		// コンストラクタ

		public CAct演奏判定文字列共通()
		{
			this.st判定文字列 = new ST判定文字列[ 7 ];
			Rectangle[] r = new Rectangle[] {
				new Rectangle( 0, 0,    90, 60 ),		// Perfect
				new Rectangle( 0, 0,    90, 60 ),		// Great
				new Rectangle( 0, 60,   90, 60 ),		// Good
				new Rectangle( 0, 120,   90, 60 ),		// Poor
				new Rectangle( 0, 120,   90, 60 ),		// Miss
				new Rectangle( 0, 180,   90, 60 ),		// Bad
				new Rectangle( 0, 0,    90, 60 )		// Auto
			};
			for ( int i = 0; i < 7; i++ )
			{
				this.st判定文字列[ i ] = new ST判定文字列();
				this.st判定文字列[ i ].rc = r[i];
			}
			base.b活性化してない = true;
		}


		// メソッド

		public virtual void Start( int nLane, E判定 judge, int lag, CDTX.CChip pChip, int player )
		{
			// When performing calibration, reduce visual distraction
			// and current judgment feedback near the judgment position.
			if (TJAPlayer3.IsPerformingCalibration)
			{
				return;
			}

			if( pChip.nチャンネル番号 >= 0x15 && pChip.nチャンネル番号 <= 0x19 )
			{
				return;
			}


			for (int i = 0; i < 1; i++)
			{
				for (int j = 0; j < 12; j++)
				{
					if (this.st状態[j].b使用中 == false)
					{
						this.st状態[j].ct進行 = new CCounter(0, 300, 1, TJAPlayer3.Timer);
						this.st状態[j].b使用中 = true;
						this.st状態[j].judge = judge;
						this.st状態[j].n相対X座標 = 0;
						this.st状態[j].n相対Y座標 = 0;
						this.st状態[j].n透明度 = 0xff;
						this.st状態[j].nPlayer = player;
						break;
					}

				}
			}

			
		}


		// CActivity 実装

		public override void On活性化()
		{
			for( int i = 0; i < 12; i++ )
			{
				this.st状態[ i ].ct進行 = new CCounter();
				this.st状態[ i ].b使用中 = false;
			}
			base.On活性化();
		}
		public override void On非活性化()
		{
			for( int i = 0; i < 12; i++ )
			{
				this.st状態[ i ].ct進行 = null;
			}
			base.On非活性化();
		}
		public override void OnManagedリソースの作成()
		{
			if( !base.b活性化してない )
			{
				//this.tx判定文字列 = CDTXMania.tテクスチャの生成( CSkin.Path( @"Graphics\7_judgement.png" ) );
				base.OnManagedリソースの作成();
			}
		}
		public override void OnManagedリソースの解放()
		{
			if( !base.b活性化してない )
			{
				//CDTXMania.t安全にDisposeする( ref this.tx判定文字列 );
				base.OnManagedリソースの解放();
			}
		}

		public virtual int t進行描画()
		{
			return 0;
		}
	}
}
