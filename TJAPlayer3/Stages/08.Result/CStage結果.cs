﻿using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using FDK;

namespace TJAPlayer3
{
	internal class CStage結果 : CStage
	{
		// プロパティ

		public bool b新記録スキル;
		public bool b新記録ランク;
		public float fPerfect率;
		public float fGreat率;
		public float fGood率;
		public float fPoor率;
		public float fMiss率;
		public int nランク値;
		public int n総合ランク値;
		public CDTX.CChip[] r空うちドラムチップ;
		public CScoreIni.C演奏記録[] st演奏記録;


		// コンストラクタ

		public CStage結果()
		{
			this.st演奏記録 = new CScoreIni.C演奏記録[2];
			this.r空うちドラムチップ = new CDTX.CChip[ 10 ];
			this.n総合ランク値 = -1;
			base.eステージID = CStage.Eステージ.結果;
			base.eフェーズID = CStage.Eフェーズ.共通_通常状態;
			base.b活性化してない = true;
			base.list子Activities.Add( this.actParameterPanel = new CActResultParameterPanel() );
			base.list子Activities.Add( this.actSongBar = new CActResultSongBar() );
			base.list子Activities.Add( this.actFI = new CActFIFOResult() );
			base.list子Activities.Add( this.actFO = new CActFIFOBlack() );
		}

		// CStage 実装

		public override void On活性化()
		{
			Trace.TraceInformation( "結果ステージを活性化します。" );
			Trace.Indent();
			try
			{
				#region [ 初期化 ]
				//---------------------
				this.eフェードアウト完了時の戻り値 = E戻り値.継続;
				this.bアニメが完了 = false;
				this.bIsCheckedWhetherResultScreenShouldSaveOrNot = false;				// #24609 2011.3.14 yyagi
				this.b新記録スキル = false;
				this.b新記録ランク = false;
				//---------------------
				#endregion

				#region [ 結果の計算 ]
				//---------------------
				for( int i = 0; i < 1; i++ )
				{
					this.nランク値 = -1;
					this.fPerfect率 = this.fGreat率 = this.fGood率 = this.fPoor率 = this.fMiss率 = 0.0f;	// #28500 2011.5.24 yyagi
					if ( ( ( ( i != 0 ) || ( TJAPlayer3.DTX[0].bチップがある.Drums  ) ) ) )
					{
						CScoreIni.C演奏記録 part = this.st演奏記録[0];

						bool bIsAutoPlay = true;
						bIsAutoPlay = TJAPlayer3.ConfigIni.b太鼓パートAutoPlay[0];

						this.fPerfect率 = bIsAutoPlay ? 0f : ( ( 100f * part.nPerfect数 ) / ( (float) part.n全チップ数 ) );
						this.fGreat率 = bIsAutoPlay ? 0f : ( ( 100f * part.nGreat数 ) / ( (float) part.n全チップ数 ) );
						this.fGood率 = bIsAutoPlay ? 0f : ( ( 100f * part.nGood数 ) / ( (float) part.n全チップ数 ) );
						this.fPoor率 = bIsAutoPlay ? 0f : ( ( 100f * part.nPoor数 ) / ( (float) part.n全チップ数 ) );
						this.fMiss率 = bIsAutoPlay ? 0f : ( ( 100f * part.nMiss数 ) / ( (float) part.n全チップ数 ) );
						this.nランク値 = CScoreIni.tランク値を計算して返す( part );
					}
				}
				this.n総合ランク値 = CScoreIni.t総合ランク値を計算して返す( this.st演奏記録[0] );
				//---------------------
				#endregion

				#region [ .score.ini の作成と出力 ]
				//---------------------
				string str = TJAPlayer3.DTX[0].strファイル名の絶対パス + ".score.ini";
				CScoreIni ini = new CScoreIni( str );

				bool b今までにフルコンボしたことがある = false;

				for( int i = 0; i < 1; i++ )
				{
					// フルコンボチェックならびに新記録ランクチェックは、ini.Record[] が、スコアチェックや演奏型スキルチェックの IF 内で書き直されてしまうよりも前に行う。(2010.9.10)
					
					b今までにフルコンボしたことがある = ini.stセクション.HiScoreDrums.bフルコンボである | ini.stセクション.HiSkillDrums.bフルコンボである;

					// #24459 上記の条件だと[HiSkill.***]でのランクしかチェックしていないので、BestRankと比較するよう変更。
					if ( this.nランク値 >= 0 && ini.stファイル.BestRank > this.nランク値 )		// #24459 2011.3.1 yyagi update BestRank
					{
						this.b新記録ランク = true;
						ini.stファイル.BestRank = this.nランク値;
					}

					// 新記録スコアチェック
					if( ( this.st演奏記録[0].nハイスコア[TJAPlayer3.stage選曲.n確定された曲の難易度[0]] > ini.stセクション.HiScoreDrums.nハイスコア[TJAPlayer3.stage選曲.n確定された曲の難易度[0]] ) && !TJAPlayer3.ConfigIni.b太鼓パートAutoPlay[0] )//2020.04.18 Mr-Ojii それぞれの難易度のハイスコアでハイスコアを変更するように修正
					{
						ini.stセクション.HiScoreDrums = this.st演奏記録[0];
					}

					// 新記録スキルチェック
					if (this.st演奏記録[0].db演奏型スキル値 > ini.stセクション.HiSkillDrums.db演奏型スキル値)
					{
						this.b新記録スキル = true;
						ini.stセクション.HiSkillDrums = this.st演奏記録[0];
					}

					// ラストプレイ #23595 2011.1.9 ikanick
					// オートじゃなければプレイ結果を書き込む
					if( TJAPlayer3.ConfigIni.b太鼓パートAutoPlay[0] == false ) {
						ini.stセクション.LastPlayDrums = this.st演奏記録[0];
					}

					// #23596 10.11.16 add ikanick オートじゃないならクリア回数を1増やす
					//        11.02.05 bオート to t更新条件を取得する use      ikanick
					bool b更新が必要か否か = false;
					CScoreIni.t更新条件を取得する(out b更新が必要か否か);

					if (b更新が必要か否か)
					{
						switch ( i )
						{
							case 0:
								ini.stファイル.ClearCountDrums++;
								break;
							default:
								throw new Exception("クリア回数増加のk(0)が範囲外です。");
						}
					}
					//---------------------------------------------------------------------/
				}
				if (TJAPlayer3.ConfigIni.bScoreIniを出力する)
				{
					ini.t書き出し(str);
				}
				//---------------------
				#endregion

				#region [ 選曲画面の譜面情報の更新 ]
				//---------------------

				{ 
					Cスコア cスコア = TJAPlayer3.stage選曲.r確定されたスコア;
					bool b更新が必要か否か = false;
					CScoreIni.t更新条件を取得する(out b更新が必要か否か);
					for (int m = 0; m < 1; m++)
					{
						if (b更新が必要か否か)
						{
							// FullCombo した記録を FullCombo なしで超えた場合、FullCombo マークが消えてしまう。
							// → FullCombo は、最新記録と関係なく、一度達成したらずっとつくようにする。(2010.9.11)
							cスコア.譜面情報.フルコンボ = this.st演奏記録[0].bフルコンボである | b今までにフルコンボしたことがある;

							if (this.b新記録スキル)
							{
								cスコア.譜面情報.最大スキル = this.st演奏記録[0].db演奏型スキル値;
							}

							if (this.b新記録ランク)
							{
								cスコア.譜面情報.最大ランク = this.nランク値;
							}
							cスコア.譜面情報.n王冠 = st演奏記録[0].n王冠;//2020.05.22 Mr-Ojii データが保存されない問題の解決策。
							cスコア.譜面情報.nハイスコア = st演奏記録[0].nハイスコア;
						}
					}
					TJAPlayer3.stage選曲.r確定されたスコア = cスコア;
				}
				//---------------------
				#endregion

				// Discord Presenseの更新
				Discord.UpdatePresence(TJAPlayer3.DTX[0].TITLE + TJAPlayer3.DTX[0].EXTENSION, Properties.Discord.Stage_Result + (TJAPlayer3.ConfigIni.b太鼓パートAutoPlay[0] == true ? " (" + Properties.Discord.Info_IsAuto + ")" : ""), TJAPlayer3.StartupTime);

				base.On活性化();
			}
			finally
			{
				Trace.TraceInformation( "結果ステージの活性化を完了しました。" );
				Trace.Unindent();
			}
		}
		public override void On非活性化()
		{
			if( this.rResultSound != null )
			{
				TJAPlayer3.Sound管理.tサウンドを破棄する( this.rResultSound );
				this.rResultSound = null;
			}
			base.On非活性化();
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
				if( this.ct登場用 != null )
				{
					this.ct登場用 = null;
				}
				base.OnManagedリソースの解放();
			}
		}
		public override int On進行描画()
		{
			if( !base.b活性化してない )
			{
				int num;
				if( base.b初めての進行描画 )
				{
					this.ct登場用 = new CCounter( 0, 100, 5, TJAPlayer3.Timer );
					this.actFI.tフェードイン開始();
					base.eフェーズID = CStage.Eフェーズ.共通_フェードイン;
					if( this.rResultSound != null )
					{
						this.rResultSound.t再生を開始する();
					}
					base.b初めての進行描画 = false;
				}
				this.bアニメが完了 = true;
				if( this.ct登場用.b進行中 )
				{
					this.ct登場用.t進行();
					if( this.ct登場用.b終了値に達した )
					{
						this.ct登場用.t停止();
					}
					else
					{
						this.bアニメが完了 = false;
					}
				}

				// 描画

				if(TJAPlayer3.Tx.Result_Background != null )
				{
					TJAPlayer3.Tx.Result_Background.t2D描画( TJAPlayer3.app.Device, 0, 0 );
				}
				if( this.ct登場用.b進行中 && ( TJAPlayer3.Tx.Result_Header != null ) )
				{
					double num2 = ( (double) this.ct登場用.n現在の値 ) / 100.0;
					double num3 = Math.Sin( Math.PI / 2 * num2 );
					num = ( (int) ( TJAPlayer3.Tx.Result_Header.sz画像サイズ.Height * num3 ) ) - TJAPlayer3.Tx.Result_Header.sz画像サイズ.Height;
				}
				else
				{
					num = 0;
				}
				if(TJAPlayer3.Tx.Result_Header != null )
				{
					TJAPlayer3.Tx.Result_Header.t2D描画( TJAPlayer3.app.Device, 0, 0 );
				}
				if ( this.actParameterPanel.On進行描画() == 0 )
				{
					this.bアニメが完了 = false;
				}

				if ( this.actSongBar.On進行描画() == 0 )
				{
					this.bアニメが完了 = false;
				}

				#region ネームプレート
				for (int i = 0; i < TJAPlayer3.ConfigIni.nPlayerCount; i++)
				{
					if (TJAPlayer3.Tx.NamePlate[i] != null)
					{
						TJAPlayer3.Tx.NamePlate[i].t2D描画(TJAPlayer3.app.Device, TJAPlayer3.Skin.Result_NamePlate_X[i], TJAPlayer3.Skin.Result_NamePlate_Y[i]);
					}
				}
				#endregion

				if ( base.eフェーズID == CStage.Eフェーズ.共通_フェードイン )
				{
					if( this.actFI.On進行描画() != 0 )
					{
						base.eフェーズID = CStage.Eフェーズ.共通_通常状態;
						var random = rng.Next(1, 99);

						if (random <= 24) this.bgm結果画面 = new CSkin.Cシステムサウンド(@"Sounds\Result BGM.ogg", false, false, ESoundGroup.SongPlayback);
						else if (random <= 50 && random >= 25) this.bgm結果画面 = new CSkin.Cシステムサウンド(@"Sounds\Result BGM_1.ogg", false, false, ESoundGroup.SongPlayback);
						else if (random <= 75 && random >= 51) this.bgm結果画面 = new CSkin.Cシステムサウンド(@"Sounds\Result BGM_2.ogg", false, false, ESoundGroup.SongPlayback);
						//else if (random <= 99 && random >= 76) this.bgm結果画面 = new CSkin.Cシステムサウンド(@"Sounds\Result BGM_3.ogg", false, false, ESoundGroup.SongPlayback);
						this.bgm結果画面.t再生する();
						this.sound結果画面out = new CSkin.Cシステムサウンド(@"Sounds\Result out.ogg", false, false, ESoundGroup.SoundEffect);
					}
				}
				else if( ( base.eフェーズID == CStage.Eフェーズ.共通_フェードアウト ) )			//&& ( this.actFO.On進行描画() != 0 ) )
				{
					return (int) this.eフェードアウト完了時の戻り値;
				}
				#region [ #24609 2011.3.14 yyagi ランク更新or演奏型スキル更新時、リザルト画像をpngで保存する ]
				if ( this.bアニメが完了 == true && this.bIsCheckedWhetherResultScreenShouldSaveOrNot == false	// #24609 2011.3.14 yyagi; to save result screen in case BestRank or HiSkill.
					&& TJAPlayer3.ConfigIni.bScoreIniを出力する
					&& TJAPlayer3.ConfigIni.bIsAutoResultCapture)												// #25399 2011.6.9 yyagi
				{
					CheckAndSaveResultScreen(true);
					this.bIsCheckedWhetherResultScreenShouldSaveOrNot = true;
				}
				#endregion

				// キー入力

				if ((TJAPlayer3.Input管理.Keyboard.bキーが押された((int)SlimDXKeys.Key.Return) || TJAPlayer3.Pad.b押された(Eパッド.LRed) || TJAPlayer3.Pad.b押された(Eパッド.RRed) || (TJAPlayer3.Pad.b押された(Eパッド.LRed2P) || TJAPlayer3.Pad.b押された(Eパッド.RRed2P)) && TJAPlayer3.ConfigIni.nPlayerCount >= 2) && !this.bアニメが完了)
				{
					this.actFI.tフェードイン完了();                 // #25406 2011.6.9 yyagi
					this.actParameterPanel.tアニメを完了させる();
					this.actSongBar.tアニメを完了させる();
					this.ct登場用.t停止();
				}
				if (base.eフェーズID == CStage.Eフェーズ.共通_通常状態)
				{
					if (TJAPlayer3.Input管理.Keyboard.bキーが押された((int)SlimDXKeys.Key.Escape))
					{
						this.bgm結果画面.t停止する();
						this.sound結果画面out.t再生する();
						this.actFO.tフェードアウト開始();
						base.eフェーズID = CStage.Eフェーズ.共通_フェードアウト;
						this.eフェードアウト完了時の戻り値 = E戻り値.完了;
					}
					if ((TJAPlayer3.Input管理.Keyboard.bキーが押された((int)SlimDXKeys.Key.Return) || TJAPlayer3.Pad.b押された(Eパッド.LRed) || TJAPlayer3.Pad.b押された(Eパッド.RRed) || (TJAPlayer3.Pad.b押された(Eパッド.LRed2P) || TJAPlayer3.Pad.b押された(Eパッド.RRed2P)) && TJAPlayer3.ConfigIni.nPlayerCount >= 2) && this.bアニメが完了)
					{
						this.bgm結果画面.t停止する();
						this.sound結果画面out.t再生する();
						//							this.actFO.tフェードアウト開始();
						base.eフェーズID = CStage.Eフェーズ.共通_フェードアウト;
						this.eフェードアウト完了時の戻り値 = E戻り値.完了;
					}
				}
				
			}
			return 0;
		}

		public enum E戻り値 : int
		{
			継続,
			完了
		}


		// その他

		#region [ private ]
		//-----------------
		private CCounter ct登場用;
		private E戻り値 eフェードアウト完了時の戻り値;
		private CActFIFOResult actFI;
		private CActFIFOBlack actFO;
		private CActResultParameterPanel actParameterPanel;
		private CActResultSongBar actSongBar;
		private bool bアニメが完了;
		private bool bIsCheckedWhetherResultScreenShouldSaveOrNot;              // #24509 2011.3.14 yyagi
		public CSkin.Cシステムサウンド bgm結果画面 = null;
		public CSkin.Cシステムサウンド sound結果画面out = null;
		private Random rng = new System.Random();
		private CSound rResultSound;

		#region [ #24609 リザルト画像をpngで保存する ]		// #24609 2011.3.14 yyagi; to save result screen in case BestRank or HiSkill.
		/// <summary>
		/// リザルト画像のキャプチャと保存。
		/// 自動保存モード時は、ランク更新or演奏型スキル更新時に自動保存。
		/// 手動保存モード時は、ランクに依らず保存。
		/// </summary>
		/// <param name="bIsAutoSave">true=自動保存モード, false=手動保存モード</param>
		private void CheckAndSaveResultScreen(bool bIsAutoSave)
		{
			string path = Path.GetDirectoryName( TJAPlayer3.DTX[0].strファイル名の絶対パス );
			string datetime = DateTime.Now.ToString( "yyyyMMddHHmmss" );
			if ( bIsAutoSave )
			{
				// リザルト画像を自動保存するときは、dtxファイル名.yyMMddHHmmss_SS.png という形式で保存。

				if (this.b新記録ランク == true || this.b新記録スキル == true)
				{
					string strRank = ((CScoreIni.ERANK)(this.nランク値)).ToString();
					string strFullPath = TJAPlayer3.DTX[0].strファイル名の絶対パス + "." + datetime + "_" + strRank + ".png";

					TJAPlayer3.app.SaveResultScreen(strFullPath);
				}
			}
		}
		#endregion
		//-----------------
		#endregion
	}
}
