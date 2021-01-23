using FDK;

namespace TJAPlayer3
{
	internal class CAct演奏Drumsスコア : CAct演奏スコア共通
	{
		// CActivity 実装（共通クラスからの差分のみ）
		public unsafe override int On進行描画()
		{
			for (int i = 0; i < 256; i++)
			{
				TJAPlayer3.act文字コンソール.tPrint(0, 14, C文字コンソール.Eフォント種別.白, string.Format("{0,7:######0}", this.n現在の本当のスコア[0]));
				if (this.stScore[i].b使用中)
				{
					if (!this.stScore[i].ctTimer.b停止中)
					{
						this.stScore[i].ctTimer.t進行();
						if (this.stScore[i].ctTimer.b終了値に達した)
						{
							this.n現在表示中のスコア[this.stScore[i].nPlayer] += (long)this.stScore[i].nAddScore;
							if (this.stScore[i].b表示中 == true)
								this.n現在表示中のAddScore--;
							this.stScore[i].ctTimer.t停止();
							this.stScore[i].b使用中 = false;
							if (ct点数アニメタイマ[stScore[i].nPlayer].b終了値に達してない)
							{
								this.ct点数アニメタイマ[stScore[i].nPlayer] = new CCounter(0, 5, 5, TJAPlayer3.Timer);
								this.ct点数アニメタイマ[stScore[i].nPlayer].n現在の値 = 1;
							}
							else
							{
								this.ct点数アニメタイマ[stScore[i].nPlayer] = new CCounter(0, 5, 5, TJAPlayer3.Timer);
							}
							TJAPlayer3.stage演奏ドラム画面.actDan.Update();
						}
						TJAPlayer3.act文字コンソール.tPrint(0, 0, C文字コンソール.Eフォント種別.赤, string.Format("{0,7:######0}", this.stScore[i].nAddScore));
					}
				}
			}
			return 0;
		}
	}
}
	