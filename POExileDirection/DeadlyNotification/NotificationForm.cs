﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Media;
using WindowsInput;
using WindowsInput.Native;
using System.Reflection;

namespace POExileDirection
{
    public partial class NotificationForm : Form
    {
        public static string panelName { get; set; }

        private MediaPlayer _mediaPlayer; // Added 1.3.9.0 Ver.

        ITEMIndicatorForm itemIndicator = null;
        private bool bIndicatorShowing = false;

        private int nMoving = 0;
        private int nMovePosX = 0;
        private int nMovePosY = 0;

        DeadlyTRADE.TradeMSG thisTradeMsg = null;

        private bool bIsQuadStash = false;
        private bool bIsExpanded = false;
        private bool bIsMinimized = false;

        private double oneExaltedChaosNotify = 0.0;

        private int diffCNT = 0;
        private DateTime rcvDateTime = DateTime.Now;

        string strBmpPath = String.Empty;
        public NotificationForm()
        {
            InitializeComponent();
            Text = "DeadlyTradeForPOE";
        }

        private void NotificationForm_Load(object sender, EventArgs e)
        {
            Visible = false;
            this.StartPosition = FormStartPosition.Manual;

            SetFormStyle();
            Init_Controls();
            /*
            public static List<DeadlyTRADE.TradeMSG> g_TradeMsgList { get; set; } //= new List<DeadlyTRADE.TradeMSG>();
            public static int g_nNotificationShownCNT { get; set; }
            public static int g_nNotificationBuyingCount { get; set; }
            public static int g_nNotificationPanelShownCNT { get; set; }
            */

            thisTradeMsg = ControlForm.g_TradeMsgList[ControlForm.g_TradeMsgList.Count - 1];

            #region ⨌⨌ Just comment for Ref. Definition ⨌⨌
            /*
            // 알림 패널 정보
            public string id { get; set; }
            public bool expanded { get; set; }

            // 구매, 판매 정보
            public string tradePurpose { get; set; } // 거래 목적 : 구매? 판매?

            // 기본 정보
            public string fullMSG { get; set; } // 전체 메시지
            public string league { get; set; } // 전체 메시지
            public string nickName { get; set; } // 누가
            public string tabName { get; set; } // 어떤 보관함의
            public string xPos { get; set; } // 가로 좌표
            public string yPos { get; set; } // 세로 좌표
            public string itemName { get; set; } // 어떤 아이템을

            // 메시지 보낸 사람이 내는 가격
            public string priceCall { get; set; } // 얼마의 (요청자) :: 나의 170
            public string whichCurrency { get; set; } // 어떤 커런시로 (요청자) :: 나의 Chaos Orb 로

            // 추가 메시지
            public string offerMSG { get; set; } // 추가로 할말과 함께

            // 메시지 받는 사람에게 원하는 커런시
            public string priceYour { get; set; } // 대상자의 얼마를 :: 너의 1
            public string yourCurrency { get; set; } // 대상자의 어떤 커런시를 :: 너의 Exalted Orb 를
            */
            #endregion

            ControlForm.g_nNotificationPanelShownCNT = ControlForm.g_nNotificationPanelShownCNT + 1;
            diffCNT = ControlForm.g_nNotificationPanelShownCNT;
            Show_TradeInformation(thisTradeMsg);

            Visible = true;
        }

        private void PlayMediaFile(string filename)
        {
            _mediaPlayer = new MediaPlayer();
            _mediaPlayer.Open(new Uri(filename));

            SetVolume(LauncherForm.g_NotifyVolume);
            _mediaPlayer.Play();
            _mediaPlayer = null;
        }

        public void SetVolume(int volume)
        {
            _mediaPlayer.Volume = volume / 100.0f; // MediaPlayer volume is a float value between 0 and 1.
        }

        private void Show_TradeInformation(DeadlyTRADE.TradeMSG tradeItem)
        {
            if(thisTradeMsg.tradePurpose=="BUY")
            {
                checkQuadTab.Visible = false;

                labelPriceAtTitle.Left = 31;
                btnCurrency.Left = 78;
                pictureArrow.Left = 103;
                labelItemName.Left = 126;

                btnHideout.Visible = true;
                pictureHideoutVert.Visible = true; 
                DeadlyToolTip.SetToolTip(this.btnKick, "Leave Party");
            }
            else
            {
                labelItemName.Left = 31;
                pictureArrow.Left = 237;
                labelPriceAtTitle.Left = 259;
                btnCurrency.Left = 308;

                btnHideout.Visible = false;
                pictureHideoutVert.Visible = false;
                DeadlyToolTip.SetToolTip(this.btnKick, "Kick From Party");
            }
            double nCalcRet = 0;
            double dSelectedCurr = 0;
            ControlForm.CurrencyCalcDictionary.TryGetValue("Exalted Orb", out dSelectedCurr);
            labelLeague.Text = String.Format("{0} (1ex={1}c)", tradeItem.league, dSelectedCurr.ToString("N1"));

            //if (labelNickName.Text.Length > 13)
            //    labelNickName.Text = tradeItem.nickName.Substring(0, 12);
            //else
                labelNickName.Text = tradeItem.nickName;
            //if (labelItemName.Text.Length > 25)
            //    labelItemName.Text = tradeItem.itemName.Substring(0, 24);
            //else
                labelItemName.Text = tradeItem.itemName;
            string strTabDetail = String.Format("({0}, {1}) {2}", tradeItem.xPos, tradeItem.yPos, tradeItem.tabName);
            labelStashTabDetail.Text = strTabDetail;
            labelPrice.Text = tradeItem.priceCall;
            labelPriceAtTitle.Text = tradeItem.priceCall;

            string strCurreny = String.Empty;

            bool bIsExpansive = false;
            bool bFoundCurrencyImage = false;
            #region ⨌⨌ Currency Character, Image ⨌⨌
            try
            {
                nCalcRet = 0;
                dSelectedCurr = 0;
                if (tradeItem.whichCurrency.Contains("Chaos") || tradeItem.whichCurrency.Contains("Cha") ||
                    tradeItem.whichCurrency.Contains("chaos") || tradeItem.whichCurrency.Contains("cha"))
                {
                    strBmpPath = @".\CurrencyImage\Chaos Orb.png";
                    pictureCurrency.BackgroundImage = Bitmap.FromFile(@".\CurrencyImage\Chaos Orb.png");
                    strCurreny = "Chaos Orb";

                    ControlForm.CurrencyCalcDictionary.TryGetValue("Exalted Orb", out dSelectedCurr);
                    nCalcRet = Convert.ToDouble(Convert.ToDouble(tradeItem.priceCall) / dSelectedCurr);
                    labelPrice.Text = "(" + nCalcRet.ToString("N1") + "e) " + tradeItem.priceCall;
                    if (nCalcRet >= 1.0)
                        bIsExpansive = true;

                    bFoundCurrencyImage = true;
                }

                if (tradeItem.whichCurrency.Contains("Alch") || tradeItem.whichCurrency.Contains("Alc") ||
                    tradeItem.whichCurrency.Contains("alch") || tradeItem.whichCurrency.Contains("alc"))
                {
                    strBmpPath = @".\CurrencyImage\alchemy.png";
                    pictureCurrency.BackgroundImage = Bitmap.FromFile(@".\CurrencyImage\alchemy.png");
                    strCurreny = "Orb of Alchemy";

                    bFoundCurrencyImage = true;
                }

                if (tradeItem.whichCurrency.Contains("Alte") || tradeItem.whichCurrency.Contains("Alt") ||
                    tradeItem.whichCurrency.Contains("alte") || tradeItem.whichCurrency.Contains("alt"))
                {
                    strBmpPath = @".\CurrencyImage\alteration.png";
                    pictureCurrency.BackgroundImage = Bitmap.FromFile(@".\CurrencyImage\alteration.png");
                    strCurreny = "Orb of Alteration";

                    bFoundCurrencyImage = true;
                }

                if (tradeItem.whichCurrency.Contains("Peran") || tradeItem.whichCurrency.Contains("Pera") ||
                    tradeItem.whichCurrency.Contains("peran") || tradeItem.whichCurrency.Contains("pera"))
                {
                    strBmpPath = @".\CurrencyImage\coin.png";
                    pictureCurrency.BackgroundImage = Bitmap.FromFile(@".\CurrencyImage\coin.png");
                    strCurreny = "Perandus Coin";

                    bFoundCurrencyImage = true;
                }

                if (tradeItem.whichCurrency.Contains("Anci") || tradeItem.whichCurrency.Contains("Anc") ||
                    tradeItem.whichCurrency.Contains("anci") || tradeItem.whichCurrency.Contains("anc"))
                {
                    strBmpPath = @".\CurrencyImage\Ancient Orb.png";
                    pictureCurrency.BackgroundImage = Bitmap.FromFile(@".\CurrencyImage\Ancient Orb.png");
                    strCurreny = "Ancient Orb";

                    bFoundCurrencyImage = true;
                }

                if (tradeItem.whichCurrency.Contains("Blessed") || tradeItem.whichCurrency.Contains("Blesse") ||
                    tradeItem.whichCurrency.Contains("blessed") || tradeItem.whichCurrency.Contains("blesse"))
                {
                    strBmpPath = @".\CurrencyImage\blessed.png";
                    pictureCurrency.BackgroundImage = Bitmap.FromFile(@".\CurrencyImage\blessed.png");
                    strCurreny = "Blessed Orb";

                    bFoundCurrencyImage = true;
                }

                if (tradeItem.whichCurrency.Contains("Chance") || tradeItem.whichCurrency.Contains("chanc") ||
                    tradeItem.whichCurrency.Contains("chance") || tradeItem.whichCurrency.Contains("chanc"))
                {
                    strBmpPath = @".\CurrencyImage\chance.png";
                    pictureCurrency.BackgroundImage = Bitmap.FromFile(@".\CurrencyImage\chance.png");
                    strCurreny = "Orb of Chance";

                    bFoundCurrencyImage = true;
                }

                if (tradeItem.whichCurrency.Contains("Chis") || tradeItem.whichCurrency.Contains("Chi") ||
                    tradeItem.whichCurrency.Contains("chis") || tradeItem.whichCurrency.Contains("chi"))
                {
                    strBmpPath = @".\CurrencyImage\chisel.png";
                    pictureCurrency.BackgroundImage = Bitmap.FromFile(@".\CurrencyImage\chisel.png");
                    strCurreny = "Cartographer's Chisel";

                    bFoundCurrencyImage = true;
                }

                if (tradeItem.whichCurrency.Contains("Chro") || tradeItem.whichCurrency.Contains("Chr") ||
                    tradeItem.whichCurrency.Contains("chro") || tradeItem.whichCurrency.Contains("chr"))
                {
                    strBmpPath = @".\CurrencyImage\chromatic.png";
                    pictureCurrency.BackgroundImage = Bitmap.FromFile(@".\CurrencyImage\chromatic.png");
                    strCurreny = "Chromatic Orb";

                    bFoundCurrencyImage = true;
                }

                if (tradeItem.whichCurrency.Contains("Divi") || tradeItem.whichCurrency.Contains("Div") ||
                    tradeItem.whichCurrency.Contains("divi") || tradeItem.whichCurrency.Contains("div"))
                {
                    strBmpPath = @".\CurrencyImage\divine.png";
                    pictureCurrency.BackgroundImage = Bitmap.FromFile(@".\CurrencyImage\divine.png");
                    strCurreny = "Divine Orb";

                    bFoundCurrencyImage = true;

                    if (tradeItem.whichCurrency.Contains("Vess") || tradeItem.whichCurrency.Contains("ves") ||
                    tradeItem.whichCurrency.Contains("vess") || tradeItem.whichCurrency.Contains("ves"))
                    {
                        strBmpPath = @".\CurrencyImage\Divine Vessel.png";
                        pictureCurrency.BackgroundImage = Bitmap.FromFile(@".\CurrencyImage\Divine Vessel.png");
                        strCurreny = "Divine Vessel";
                    }
                }

                if (tradeItem.whichCurrency.Contains("Engi") || tradeItem.whichCurrency.Contains("Eng") ||
                    tradeItem.whichCurrency.Contains("engi") || tradeItem.whichCurrency.Contains("eng"))
                {
                    strBmpPath = @".\CurrencyImage\Engineer's Orb.png";
                    pictureCurrency.BackgroundImage = Bitmap.FromFile(@".\CurrencyImage\Engineer's Orb.png");
                    strCurreny = "Engineer's Orb";

                    bFoundCurrencyImage = true;
                }

                if (tradeItem.whichCurrency.Contains("Exal") || tradeItem.whichCurrency.Contains("Exa") ||
                    tradeItem.whichCurrency.Contains("exal") || tradeItem.whichCurrency.Contains("exa"))
                {
                    strBmpPath = @".\CurrencyImage\Exalted Orbs.png";
                    pictureCurrency.BackgroundImage = Bitmap.FromFile(@".\CurrencyImage\Exalted Orbs.png");
                    strCurreny = "Exalted Orb";

                    bFoundCurrencyImage = true;

                    if (tradeItem.whichCurrency.Contains("Shar") || tradeItem.whichCurrency.Contains("Sha") ||
                    tradeItem.whichCurrency.Contains("shar") || tradeItem.whichCurrency.Contains("sha"))
                    {
                        strBmpPath = @".\CurrencyImage\Exalted Shard.png";
                        pictureCurrency.BackgroundImage = Bitmap.FromFile(@".\CurrencyImage\Exalted Shard.png");
                        strCurreny = "Exalted Shard";
                    }

                    if (strCurreny == "Exalted Orb")
                    {
                        ControlForm.CurrencyCalcDictionary.TryGetValue("Exalted Orb", out dSelectedCurr);
                        nCalcRet = Convert.ToDouble(dSelectedCurr * Convert.ToDouble(tradeItem.priceCall));
                        labelPrice.Text = "(" + nCalcRet.ToString("N1") + "c)" + tradeItem.priceCall;
                    }

                    if (Convert.ToDouble(tradeItem.priceCall) >= 1.0)
                        bIsExpansive = true;
                }

                if (tradeItem.whichCurrency.Contains("Fus") || tradeItem.whichCurrency.Contains("Fu") ||
                    tradeItem.whichCurrency.Contains("fus") || tradeItem.whichCurrency.Contains("fu"))
                {
                    strBmpPath = @".\CurrencyImage\fusing.png";
                    pictureCurrency.BackgroundImage = Bitmap.FromFile(@".\CurrencyImage\fusing.png");
                    strCurreny = "Orb of Fusing";

                    bFoundCurrencyImage = true;
                }

                if (tradeItem.whichCurrency.Contains("Gem") || tradeItem.whichCurrency.Contains("Ge") ||
                    tradeItem.whichCurrency.Contains("gem") || tradeItem.whichCurrency.Contains("ge") ||
                    tradeItem.whichCurrency.Contains("GCP") || tradeItem.whichCurrency.Contains("gcp") || tradeItem.whichCurrency.Contains("Gcp"))
                {
                    strBmpPath = @".\CurrencyImage\gcp.png";
                    pictureCurrency.BackgroundImage = Bitmap.FromFile(@".\CurrencyImage\gcp.png");
                    strCurreny = "Gemcutter's Prism";

                    bFoundCurrencyImage = true;
                }

                if (tradeItem.whichCurrency.Contains("Jewe") || tradeItem.whichCurrency.Contains("Jew") ||
                    tradeItem.whichCurrency.Contains("jewe") || tradeItem.whichCurrency.Contains("jew"))
                {
                    strBmpPath = @".\CurrencyImage\jeweller's.png";
                    pictureCurrency.BackgroundImage = Bitmap.FromFile(@".\CurrencyImage\jeweller's.png");
                    strCurreny = "Jeweller's Orb";

                    bFoundCurrencyImage = true;
                }

                if (tradeItem.whichCurrency.Contains("Mirr") || tradeItem.whichCurrency.Contains("Mir") ||
                    tradeItem.whichCurrency.Contains("mirr") || tradeItem.whichCurrency.Contains("mir"))
                {
                    strBmpPath = @".\CurrencyImage\mirror.png";
                    pictureCurrency.BackgroundImage = Bitmap.FromFile(@".\CurrencyImage\mirror.png");
                    strCurreny = "Mirror of Kalandra";

                    bFoundCurrencyImage = true;

                    if (tradeItem.whichCurrency.Contains("Shar") || tradeItem.whichCurrency.Contains("Sha") ||
                    tradeItem.whichCurrency.Contains("shar") || tradeItem.whichCurrency.Contains("sha"))
                    {
                        strBmpPath = @".\CurrencyImage\Mirror Shard.png";
                        pictureCurrency.BackgroundImage = Bitmap.FromFile(@".\CurrencyImage\Mirror Shard.png");
                        strCurreny = "Mirror Shard";
                    }

                    bIsExpansive = true;
                }

                if (tradeItem.whichCurrency.Contains("Annu") || tradeItem.whichCurrency.Contains("Ann") ||
                    tradeItem.whichCurrency.Contains("annu") || tradeItem.whichCurrency.Contains("ann"))
                {
                    strBmpPath = @".\CurrencyImage\Orb of Annulment.png";
                    pictureCurrency.BackgroundImage = Bitmap.FromFile(@".\CurrencyImage\Orb of Annulment.png");
                    strCurreny = "Orb of Annulment";

                    bFoundCurrencyImage = true;
                }

                if (tradeItem.whichCurrency.Contains("Bind") || tradeItem.whichCurrency.Contains("Bin") ||
                    tradeItem.whichCurrency.Contains("bind") || tradeItem.whichCurrency.Contains("bin"))
                {
                    strBmpPath = @".\CurrencyImage\Orb of Binding.png";
                    pictureCurrency.BackgroundImage = Bitmap.FromFile(@".\CurrencyImage\Orb of Binding.png");
                    strCurreny = "Orb of Binding";

                    bFoundCurrencyImage = true;
                }

                if (tradeItem.whichCurrency.Contains("Hori") || tradeItem.whichCurrency.Contains("Hor") ||
                    tradeItem.whichCurrency.Contains("hori") || tradeItem.whichCurrency.Contains("hor"))
                {
                    strBmpPath = @".\CurrencyImage\Orb of Horizons.png";
                    pictureCurrency.BackgroundImage = Bitmap.FromFile(@".\CurrencyImage\Orb of Horizons.png");
                    strCurreny = "Orb of Horizons";

                    bFoundCurrencyImage = true;
                }

                if (tradeItem.whichCurrency.Contains("Portal") || tradeItem.whichCurrency.Contains("Porta") ||
                    tradeItem.whichCurrency.Contains("portal") || tradeItem.whichCurrency.Contains("porta"))
                {
                    strBmpPath = @".\CurrencyImage\portal.png";
                    pictureCurrency.BackgroundImage = Bitmap.FromFile(@".\CurrencyImage\portal.png");
                    strCurreny = "Portal Scroll";

                    bFoundCurrencyImage = true;
                }

                if (tradeItem.whichCurrency.Contains("Regal") || tradeItem.whichCurrency.Contains("Rega") ||
                    tradeItem.whichCurrency.Contains("regal") || tradeItem.whichCurrency.Contains("rega"))
                {
                    strBmpPath = @".\CurrencyImage\regal.png";
                    pictureCurrency.BackgroundImage = Bitmap.FromFile(@".\CurrencyImage\regal.png");
                    strCurreny = "Regal Orb";

                    bFoundCurrencyImage = true;
                }

                if (tradeItem.whichCurrency.Contains("Regre") || tradeItem.whichCurrency.Contains("Regr") ||
                    tradeItem.whichCurrency.Contains("regre") || tradeItem.whichCurrency.Contains("regr"))
                {
                    strBmpPath = @".\CurrencyImage\regret.png";
                    pictureCurrency.BackgroundImage = Bitmap.FromFile(@".\CurrencyImage\regret.png");
                    strCurreny = "Orb of Regret";

                    bFoundCurrencyImage = true;
                }

                if (tradeItem.whichCurrency.Contains("Scou") || tradeItem.whichCurrency.Contains("Sco") ||
                    tradeItem.whichCurrency.Contains("scou") || tradeItem.whichCurrency.Contains("sco"))
                {
                    strBmpPath = @".\CurrencyImage\scouring.png";
                    pictureCurrency.BackgroundImage = Bitmap.FromFile(@".\CurrencyImage\scouring.png");
                    strCurreny = "Orb of Scouring";

                    bFoundCurrencyImage = true;
                }

                if (tradeItem.whichCurrency.Contains("Silv") || tradeItem.whichCurrency.Contains("Sil") ||
                    tradeItem.whichCurrency.Contains("silv") || tradeItem.whichCurrency.Contains("sil"))
                {
                    strBmpPath = @".\CurrencyImage\silver.png";
                    pictureCurrency.BackgroundImage = Bitmap.FromFile(@".\CurrencyImage\silver.png");
                    strCurreny = "Silver Coin";

                    bFoundCurrencyImage = true;
                }

                if (tradeItem.whichCurrency.Contains("Trans") || tradeItem.whichCurrency.Contains("Tran") ||
                    tradeItem.whichCurrency.Contains("trnas") || tradeItem.whichCurrency.Contains("tran"))
                {
                    strBmpPath = @".\CurrencyImage\transmutation.png";
                    pictureCurrency.BackgroundImage = Bitmap.FromFile(@".\CurrencyImage\transmutation.png");
                    strCurreny = "Orb of Transmutation";

                    bFoundCurrencyImage = true;
                }

                if (tradeItem.whichCurrency.Contains("Armo") || tradeItem.whichCurrency.Contains("Arm") ||
                    tradeItem.whichCurrency.Contains("armo") || tradeItem.whichCurrency.Contains("arm"))
                {
                    strBmpPath = @".\CurrencyImage\armourer's.png";
                    pictureCurrency.BackgroundImage = Bitmap.FromFile(@".\CurrencyImage\armourer's.png");
                    strCurreny = "Armourer's Scrap";

                    bFoundCurrencyImage = true;
                }

                if (tradeItem.whichCurrency.Contains("Augm") || tradeItem.whichCurrency.Contains("Aug") ||
                    tradeItem.whichCurrency.Contains("augm") || tradeItem.whichCurrency.Contains("aug"))
                {
                    strBmpPath = @".\CurrencyImage\augmentation.png";
                    pictureCurrency.BackgroundImage = Bitmap.FromFile(@".\CurrencyImage\augmentation.png");
                    strCurreny = "Orb of Augmentation";

                    bFoundCurrencyImage = true;
                }

                if (tradeItem.whichCurrency.Contains("Vaal") || tradeItem.whichCurrency.Contains("Vaa") ||
                    tradeItem.whichCurrency.Contains("vaal") || tradeItem.whichCurrency.Contains("vaa"))
                {
                    strBmpPath = @".\CurrencyImage\vaal.png";
                    pictureCurrency.BackgroundImage = Bitmap.FromFile(@".\CurrencyImage\vaal.png"); // Not MAP!
                    strCurreny = "Vaal Orb";

                    bFoundCurrencyImage = true;
                }

                if (tradeItem.whichCurrency.Contains("Whet") || tradeItem.whichCurrency.Contains("Whe") ||
                    tradeItem.whichCurrency.Contains("whet") || tradeItem.whichCurrency.Contains("whe"))
                {
                    strBmpPath = @".\CurrencyImage\whetstone.png";
                    pictureCurrency.BackgroundImage = Bitmap.FromFile(@".\CurrencyImage\whetstone.png");
                    strCurreny = "Blacksmith's Whetstone";

                    bFoundCurrencyImage = true;
                }

                if (tradeItem.whichCurrency.Contains("Wisd") || tradeItem.whichCurrency.Contains("Wis") ||
                    tradeItem.whichCurrency.Contains("wisd") || tradeItem.whichCurrency.Contains("wis"))
                {
                    strBmpPath = @".\CurrencyImage\wisdom.png";
                    pictureCurrency.BackgroundImage = Bitmap.FromFile(@".\CurrencyImage\wisdom.png");
                    strCurreny = "Scroll of Wisdom";

                    bFoundCurrencyImage = true;
                }

                btnCurrency.BackgroundImage = pictureCurrency.BackgroundImage;
                if (bIsExpansive)
                {
                    labelNickName.ForeColor = System.Drawing.Color.FromArgb(0, 124, 255, 127);
                    labelStashTabDetail.ForeColor = System.Drawing.Color.FromArgb(0, 124, 255, 127);
                    labelPrice.ForeColor = System.Drawing.Color.FromArgb(0, 124, 255, 127);
                    PlayMediaFile(Application.StartupPath + "\\notify.wav");
                }
                else
                {
                    labelNickName.ForeColor = System.Drawing.Color.FromArgb(0, 255, 200, 124);
                    labelStashTabDetail.ForeColor = System.Drawing.Color.FromArgb(0, 255, 200, 124);
                    labelPrice.ForeColor = System.Drawing.Color.FromArgb(0, 255, 200, 124);
                    PlayMediaFile(Application.StartupPath + "\\notify.wav");
                }
            }
            catch (Exception ex)
            {
                DeadlyLog4Net._log.Error($"catch {MethodBase.GetCurrentMethod().Name}", ex);
            }

            if (bFoundCurrencyImage)
                btnCurrency.Text = "";
            else
                btnCurrency.Text = strCurreny.Substring(0,1);
            #endregion

            try
            {
                labelLeague.Text = tradeItem.league;
                if (tradeItem.offerMSG.Length > 0 && !String.IsNullOrEmpty(tradeItem.offerMSG))
                    labelStashTabDetail.Text = labelStashTabDetail.Text + "( "+ tradeItem.offerMSG + " )";

            }
            catch(Exception ex)
            {
                DeadlyLog4Net._log.Error($"catch {MethodBase.GetCurrentMethod().Name}", ex);
            }

            if (Convert.ToInt32(tradeItem.xPos) > 12 || Convert.ToInt32(tradeItem.yPos) > 12)
            {
                checkQuadTab.Checked = true;
                bIsQuadStash = true;
            }
            else
            {
                checkQuadTab.Checked = false;
                bIsQuadStash = false;
            }

            rcvDateTime = DateTime.Now;

            timer1.Start();
        }


        private void SetFormStyle()
        {
            this.ControlBox = false;
        }

        #region ⨌⨌ Init. Controls ⨌⨌
        private void Init_Controls()
        {
            DeadlyToolTip.SetToolTip(this.btnInvite, "Invite to Party"); 
            DeadlyToolTip.SetToolTip(this.btnTrade, "Trade Request"); 
            DeadlyToolTip.SetToolTip(this.btnClose, "Close This Message");
            DeadlyToolTip.SetToolTip(this.btnThanks, "Send 'Thanks' Message and Close Panel");
            DeadlyToolTip.SetToolTip(this.btnWaitPlz, "Send 'Plz. Wait a sec.' Message");
            DeadlyToolTip.SetToolTip(this.btnSold, "Send 'Sold already' and Close Panel");
            DeadlyToolTip.SetToolTip(this.btnHideout, "Visit Hideout");
            DeadlyToolTip.SetToolTip(this.btnWhois, "Whois? Buyer Information");
            DeadlyToolTip.SetToolTip(this.btnWilling, "Send 'Still willing to buy?' Message");
            DeadlyToolTip.SetToolTip(this.checkQuadTab, "Check if it is Quad(4x4) Stash");
            DeadlyToolTip.SetToolTip(this.btnMinMax, "Minimize or Maximaze");
            DeadlyToolTip.SetToolTip(this.btnWhisper, "Whisper (PM)");
            DeadlyToolTip.SetToolTip(this.btnWhois, "Send CMD /whois"); 

            btnInvite.FlatStyle = FlatStyle.Flat;
            btnInvite.BackColor = System.Drawing.Color.Transparent;
            btnInvite.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            btnInvite.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            btnInvite.TabStop = false;

            btnTrade.FlatStyle = FlatStyle.Flat;
            btnTrade.BackColor = System.Drawing.Color.Transparent;
            btnTrade.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            btnTrade.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            btnTrade.TabStop = false;

            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.BackColor = System.Drawing.Color.Transparent;
            btnClose.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            btnClose.TabStop = false;

            btnThanks.FlatStyle = FlatStyle.Flat;
            btnThanks.BackColor = System.Drawing.Color.Transparent;
            btnThanks.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            btnThanks.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            btnThanks.TabStop = false;

            btnWaitPlz.FlatStyle = FlatStyle.Flat;
            btnWaitPlz.BackColor = System.Drawing.Color.Transparent;
            btnWaitPlz.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            btnWaitPlz.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            btnWaitPlz.TabStop = false;

            btnSold.FlatStyle = FlatStyle.Flat;
            btnSold.BackColor = System.Drawing.Color.Transparent;
            btnSold.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            btnSold.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            btnSold.TabStop = false;

            btnHideout.FlatStyle = FlatStyle.Flat;
            btnHideout.BackColor = System.Drawing.Color.Transparent;
            btnHideout.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            btnHideout.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            btnHideout.TabStop = false;

            btnWilling.FlatStyle = FlatStyle.Flat;
            btnWilling.BackColor = System.Drawing.Color.Transparent;
            btnWilling.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            btnWilling.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            btnWilling.TabStop = false;

            btnMinMax.FlatStyle = FlatStyle.Flat;
            btnMinMax.BackColor = System.Drawing.Color.Transparent;
            btnMinMax.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            btnMinMax.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            btnMinMax.TabStop = false;

            btnCurrency.FlatStyle = FlatStyle.Flat;
            btnCurrency.BackColor = System.Drawing.Color.Transparent;
            btnCurrency.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            btnCurrency.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            btnCurrency.TabStop = false;

            btnKick.FlatStyle = FlatStyle.Flat;
            btnKick.BackColor = System.Drawing.Color.Transparent;
            btnKick.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            btnKick.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            btnKick.TabStop = false;

            btnWhisper.FlatStyle = FlatStyle.Flat;
            btnWhisper.BackColor = System.Drawing.Color.Transparent;
            btnWhisper.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            btnWhisper.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            btnWhisper.TabStop = false;

            btnWhois.FlatStyle = FlatStyle.Flat;
            btnWhois.BackColor = System.Drawing.Color.Transparent;
            btnWhois.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            btnWhois.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            btnWhois.TabStop = false;
        }
        #endregion

        private void BtnClose_Click(object sender, EventArgs e)
        {
            ControlForm.g_nNotificationPanelShownCNT = ControlForm.g_nNotificationPanelShownCNT - 1;
            ControlForm.Remove_TradeItem(thisTradeMsg.id);
            InteropCommon.SetForegroundWindow(LauncherForm.g_handlePathOfExile);
            Close();
        }


        private void btnMinMax_Click(object sender, EventArgs e)
        {
            if (!bIsMinimized)
            {
                Height = 28;
                bIsMinimized = true;
            }
            else
            {
                Height = 113;
                bIsMinimized = false;
            }
        }

        private void LabelItemName_Click(object sender, EventArgs e)
        {
            try
            {
                if (thisTradeMsg.tradePurpose != "SELL")
                    return;

                if (thisTradeMsg.tabName == "" || Convert.ToInt32(thisTradeMsg.xPos) == 0 || Convert.ToInt32(thisTradeMsg.yPos) == 0)
                    return;

                if (bIndicatorShowing)
                {
                    itemIndicator.Close();
                    bIndicatorShowing = false;

                    return;
                }

                if (checkQuadTab.Checked)
                    bIsQuadStash = true;
                else
                    bIsQuadStash = false;

                itemIndicator = new ITEMIndicatorForm();
                itemIndicator.bIsQuad = bIsQuadStash;
                itemIndicator.nStashX = Convert.ToInt32(thisTradeMsg.xPos);
                itemIndicator.nStashY = Convert.ToInt32(thisTradeMsg.yPos);
                itemIndicator._strItemName = thisTradeMsg.itemName;
                itemIndicator._strPrice = thisTradeMsg.priceCall;
                itemIndicator._strBmpPath = strBmpPath;
                itemIndicator.Owner = this;
                itemIndicator.Show();

                bIndicatorShowing = true;
            }
            catch (Exception ex)
            {
                DeadlyLog4Net._log.Error($"catch {MethodBase.GetCurrentMethod().Name}", ex);
            }
        }

        private void NotificationForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (itemIndicator != null) itemIndicator.Dispose();
            if (thisTradeMsg!=null) thisTradeMsg = null;
        }

        private void BtnInvite_Click(object sender, EventArgs e)
        {
            InteropCommon.SetForegroundWindow(LauncherForm.g_handlePathOfExile);

            InputSimulator iSim = new InputSimulator();

            iSim.Keyboard.KeyPress(VirtualKeyCode.RETURN);

            // Send Invite
            string strSendString = String.Format("/invite {0}", thisTradeMsg.nickName);
            iSim.Keyboard.TextEntry(strSendString);

            // Send RETURN
            iSim.Keyboard.KeyPress(VirtualKeyCode.RETURN);

            //iSim = null;
        }

        private void BtnKick_Click(object sender, EventArgs e)
        {
            InteropCommon.SetForegroundWindow(LauncherForm.g_handlePathOfExile);

            InputSimulator iSim = new InputSimulator();

            iSim.Keyboard.KeyPress(VirtualKeyCode.RETURN);

            string strSendString = String.Format("/kick {0}", thisTradeMsg.nickName);
            if (thisTradeMsg.tradePurpose == "BUY")
                strSendString = String.Format("/kick {0}", LauncherForm.g_strMyNickName);
            iSim.Keyboard.TextEntry(strSendString);

            // Send RETURN
            iSim.Keyboard.KeyPress(VirtualKeyCode.RETURN);

            //iSim = null;
        }

        private void BtnWilling_Click(object sender, EventArgs e)
        {
            InteropCommon.SetForegroundWindow(LauncherForm.g_handlePathOfExile);

            InputSimulator iSim = new InputSimulator();

            iSim.Keyboard.KeyPress(VirtualKeyCode.RETURN);

            string strSendString = String.Empty;
            try
            {
                if (thisTradeMsg.tradePurpose == "SELL")
                    strSendString = String.Format("@{0} {1} 'Your Offer : {2}'", thisTradeMsg.nickName, LauncherForm.g_strnotiRESEND, thisTradeMsg.fullMSG);
                else
                    strSendString = String.Format("@{0} {1}", thisTradeMsg.nickName, thisTradeMsg.fullMSG); // Send Buy Msg. (FULL)
            }
            catch
            {
                if (thisTradeMsg.tradePurpose == "SELL")
                    strSendString = String.Format("@{0} still willing to buy? 'Your Offer : {1}'", thisTradeMsg.nickName, thisTradeMsg.fullMSG);
                else
                    strSendString = String.Format("@{0} {1}", thisTradeMsg.nickName, thisTradeMsg.fullMSG); // Send Buy Msg. (FULL)
            }

            iSim.Keyboard.TextEntry(strSendString);

            // Send RETURN
            iSim.Keyboard.KeyPress(VirtualKeyCode.RETURN);

            //iSim = null;
        }

        private void BtnWhois_Click(object sender, EventArgs e)
        {
            InteropCommon.SetForegroundWindow(LauncherForm.g_handlePathOfExile);

            InputSimulator iSim = new InputSimulator();

            iSim.Keyboard.KeyPress(VirtualKeyCode.RETURN);

            string strSendString = String.Format("/whois {0}", thisTradeMsg.nickName);
            iSim.Keyboard.TextEntry(strSendString);

            // Send RETURN
            iSim.Keyboard.KeyPress(VirtualKeyCode.RETURN);

            //iSim = null;
        }

        private void BtnHideout_Click(object sender, EventArgs e)
        {
            InteropCommon.SetForegroundWindow(LauncherForm.g_handlePathOfExile);

            InputSimulator iSim = new InputSimulator();

            iSim.Keyboard.KeyPress(VirtualKeyCode.RETURN);

            string strSendString = String.Format("/hideout {0}", thisTradeMsg.nickName);
            iSim.Keyboard.TextEntry(strSendString);

            // Send RETURN
            iSim.Keyboard.KeyPress(VirtualKeyCode.RETURN);

            //iSim = null;
        }

        private void BtnTrade_Click(object sender, EventArgs e)
        {
            InteropCommon.SetForegroundWindow(LauncherForm.g_handlePathOfExile);

            InputSimulator iSim = new InputSimulator();

            iSim.Keyboard.KeyPress(VirtualKeyCode.RETURN);

            string strSendString = String.Format("/tradewith {0}", thisTradeMsg.nickName);
            iSim.Keyboard.TextEntry(strSendString);

            // Send RETURN
            iSim.Keyboard.KeyPress(VirtualKeyCode.RETURN);

            //iSim = null;
        }

        private void BtnThanks_Click(object sender, EventArgs e)
        {
            InteropCommon.SetForegroundWindow(LauncherForm.g_handlePathOfExile);

            InputSimulator iSim = new InputSimulator();

            iSim.Keyboard.KeyPress(VirtualKeyCode.RETURN);

            string strSendString = String.Empty;

            try
            {
                strSendString = String.Format("@{0} {1}", thisTradeMsg.nickName, LauncherForm.g_strnotiDONE);
            }
            catch
            {
                strSendString = String.Format("@{0} thanks. gl hf~.", thisTradeMsg.nickName);
            }

            iSim.Keyboard.TextEntry(strSendString);

            // Send RETURN
            iSim.Keyboard.KeyPress(VirtualKeyCode.RETURN);

            //iSim = null;

            if (LauncherForm.g_strTRAutoKick == "Y")
                BtnKick_Click(sender, e);

            Close();
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            DateTime nowTime = DateTime.Now;
            double elapsedsecs = ((TimeSpan)(nowTime - rcvDateTime)).TotalSeconds;
            TimeSpan tSpan = TimeSpan.FromSeconds(elapsedsecs);
            string strElapsed = string.Format("{0:D1}h {1:D2}m {2:D2}s",
                                tSpan.Hours,
                                tSpan.Minutes,
                                tSpan.Seconds);
            labelElapsed.Text = strElapsed;

            if (LauncherForm.g_pinLOCK)
                ControlForm.frmNotificationContainer.pictureMovingBar.Visible = false;
            else
            {
                ControlForm.frmNotificationContainer.pictureMovingBar.BringToFront();
                ControlForm.frmNotificationContainer.pictureMovingBar.Visible = true;
            }
        }

        private void BtnWaitPlz_Click(object sender, EventArgs e)
        {
            InteropCommon.SetForegroundWindow(LauncherForm.g_handlePathOfExile);

            InputSimulator iSim = new InputSimulator();

            iSim.Keyboard.KeyPress(VirtualKeyCode.RETURN);

            // Send Invite
            string strSendString = String.Empty;

            try
            {
                strSendString = String.Format("@{0} {1}", thisTradeMsg.nickName, LauncherForm.g_strnotiWAIT);
            }
            catch
            {
                strSendString = String.Format("@{0} wait a sec plz.", thisTradeMsg.nickName);
            }

            iSim.Keyboard.TextEntry(strSendString);

            // Send RETURN
            iSim.Keyboard.KeyPress(VirtualKeyCode.RETURN);

            //iSim = null;
        }

        private void BtnSold_Click(object sender, EventArgs e)
        {
            InteropCommon.SetForegroundWindow(LauncherForm.g_handlePathOfExile);

            InputSimulator iSim = new InputSimulator();

            iSim.Keyboard.KeyPress(VirtualKeyCode.RETURN);

            // Send Invite
            string strSendString = String.Empty;

            try
            {
                strSendString = String.Format("@{0} {1}", thisTradeMsg.nickName, LauncherForm.g_strnotiSOLD);
            }
            catch
            {
                strSendString = String.Format("@{0} sold already. Sry.", thisTradeMsg.nickName);
            }

            iSim.Keyboard.TextEntry(strSendString);

            // Send RETURN
            iSim.Keyboard.KeyPress(VirtualKeyCode.RETURN);

            //iSim = null;

            BtnClose_Click(sender, e);
        }

        private void NotificationForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape)
                return;
        }

        private void NotificationForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape)
                return;
        }

        private void btnMinimizeToMsgHolder_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Escape)
                return;
        }

        private void btnWhisper_Click(object sender, EventArgs e)
        {
            InteropCommon.SetForegroundWindow(LauncherForm.g_handlePathOfExile);

            InputSimulator iSim = new InputSimulator();

            iSim.Keyboard.KeyPress(VirtualKeyCode.RETURN);

            // Send Invite
            string strSendString = String.Empty;

            strSendString = String.Format("@{0} ", thisTradeMsg.nickName);

            iSim.Keyboard.TextEntry(strSendString);

            // Send RETURN
            //iSim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
            //iSim = null;
        }

        private void checkQuadTab_Click(object sender, EventArgs e)
        {
            if (thisTradeMsg.tradePurpose != "SELL")
                return;

            if (thisTradeMsg.tabName == "" || String.IsNullOrEmpty(thisTradeMsg.tabName) || Convert.ToInt32(thisTradeMsg.xPos) == 0 || Convert.ToInt32(thisTradeMsg.yPos) == 0)
                return;

            if (bIndicatorShowing)
            {
                itemIndicator.Close();
            }

            if (bIsQuadStash)
            {
                bIsQuadStash = false;
            }
            else
            {
                bIsQuadStash = true;
            }

            itemIndicator = new ITEMIndicatorForm();
            itemIndicator.bIsQuad = bIsQuadStash;
            itemIndicator.nStashX = Convert.ToInt32(thisTradeMsg.xPos);
            itemIndicator.nStashY = Convert.ToInt32(thisTradeMsg.yPos);
            itemIndicator._strItemName = thisTradeMsg.itemName;
            itemIndicator._strPrice = thisTradeMsg.priceCall;
            itemIndicator._strBmpPath = strBmpPath;
            itemIndicator.Owner = this;
            itemIndicator.Show();

            bIndicatorShowing = true;
        }
    }
}
