using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Osblow
{
    class UIGame : UIBase
    {
        public override string PrefabPath
        {
            get
            {
                return "ui/GameView";
            }
        }


        private Text m_confText;
        private Text m_tipsText;
        private Text m_optext;
        private Slider m_strengthSlider;
        private RectTransform m_wheel;
        private Text m_playerText;


        private HitStrength m_hitStrength;
        private RandomWheel m_randomWheel;


        public override void Start()
        {
            m_confText = GetWidget("Config").GetComponent<Text>();
            m_tipsText = GetWidget("Tips").GetComponent<Text>();
            m_optext = GetWidget("OpText").GetComponent<Text>();
            m_wheel = GetWidget("wheel").GetComponent<RectTransform>();
            m_playerText = GetWidget("CurPlayer").GetComponent<Text>();

            AddClickEvent(GetWidget("wheel/RandomOp"), OnClickRandom);
            AddClickEvent(GetWidget("RestartBtn"), OnClickRestart);
            AddClickEvent(GetWidget("ExitBtn"), OnClickExit);

            m_strengthSlider = GetWidget("Slider").GetComponent<Slider>();
            m_wheel = GetWidget("wheel/RandomOp").GetComponent<RectTransform>();
            m_hitStrength = new HitStrength(m_strengthSlider);
            m_randomWheel = new RandomWheel(m_wheel);
            ShowWheel(false);
        }

        public override void Update(float delta)
        {
            base.Update(delta);

            if (m_hitStrength != null)
            {
                m_hitStrength.Update();
            }

            if (m_randomWheel != null)
            {
                m_randomWheel.Update();
            }
        }

        
        public void SetRoomConfig()
        {
            GameConf conf = Globals.Instance.GameConf;
            m_confText.text = string.Format(
                "地图: {0}\n人数: {1}\n必须砸落: {2}",
                UIConstants.s_mapName[conf.MapType],
                conf.MemCount,
                conf.ForceKill ? "是" : "否");
        }

        public void SetCurPlayer(int index)
        {
            m_playerText.text = "玩家" + index;
        }



        public void OnClickRandom(GameObject obj, object[] args)
        {
            float rand = Random.Range(0, 100) * 0.01f;
            for (OpType i = OpType.Pass; i <= OpType.Both; i++)
            {
                if (UIConstants.s_probabilities[i] > rand)
                {
                    Globals.Instance.OpType = i;
                    break;
                }
            }
            //Debug.Log(rand + " " + Globals.Instance.OpType);


            //Globals.Instance.OpType = (OpType)UnityEngine.Random.Range(0, (int)(OpType.Both + 1));
            m_optext.text = UIConstants.s_ops[Globals.Instance.OpType];

            m_randomWheel.SetOp(Globals.Instance.OpType);
            m_randomWheel.SetActive(true);
        }


        public void OnClickExit(GameObject obj, object[] args)
        {
            Globals.Instance.EndGame();
        }

        public void OnClickRestart(GameObject obj, object[] args)
        {
            Globals.Instance.Restart();
        }

        public void OnClickHit(GameObject obj, object[] args)
        {
            //if(Globals.Instance.GameStep != GameStep.SelectingStrength)
            //{
            //    return;
            //}

            //m_hitStrength.SetEnabled(true);
            //Globals.Instance.GameStep = GameStep.Hitting;
        }


        /// <summary>
        /// 力度条开始移动
        /// </summary>
        public void StartSliderMoving()
        {
            m_hitStrength.SetEnabled(true);
        }


        /// <summary>
        /// 力度条停止，并返回当前力度（0-1）
        /// </summary>
        /// <returns>力度</returns>
        public float StopSliderMoving()
        {
            m_hitStrength.SetEnabled(false);

            return m_hitStrength.Value;
        }

        public void ResetSlider()
        {
            m_hitStrength.Reset();
        }


        public void ShowTips()
        {
            if (m_isSpecialText)
            {
                return;
            }

            m_tipsText.text = UIConstants.s_tips[Globals.Instance.GameStep];

            if (Globals.Instance.GameStep == GameStep.GameOver)
            {
                m_tipsText.text = UIConstants.s_tips[GameStep.GameOver] + "  玩家" + Globals.Instance.CurPlayer + "输了";
            }
        }

        private bool m_isSpecialText = false;
        public void ShowTips(string tips)
        {
            m_tipsText.text = tips;
            m_isSpecialText = true;

            Globals.Instance.CoroutineManager.CancelInvoke("ResetTips");
            Globals.Instance.CoroutineManager.Invoke("ResetTips", 1.5f);
        }

        private void ResetTips()
        {
            m_isSpecialText = false;
            ShowTips();
        }


        public void ShowWheel(bool isShow)
        {
            m_wheel.transform.parent.gameObject.SetActive(isShow);
        }

        public void ShowWheelLater(bool isShow, float delay)
        {
            Globals.Instance.CoroutineManager.StartCoroutine(ShowWheelLaterImp(isShow, delay));
        }

        private IEnumerator ShowWheelLaterImp(bool isShow, float delay)
        {
            yield return new WaitForSeconds(delay);
            ShowWheel(isShow);
        }
    }
}
