using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Osblow
{
    class UIPrompt : UIBase
    {
        public override string PrefabPath
        {
            get
            {
                return "ui/PromptView";
            }
        }

        public event Action<object[]> CancelDelegate;
        public event Action<object[]> ConfirmDelegate;

        public Text Title;
        private InputField m_input;


        public override void Start()
        {
            m_input = GetWidget("InputField").GetComponent<InputField>();
            Title = GetWidget("title").GetComponent<Text>();

            AddClickEvent(GetWidget("Confirm"), OnClickConfirm);
            AddClickEvent(GetWidget("Cancel"), OnClickCancel);
        }

        private void OnClickConfirm(GameObject obj, object[] args)
        {
            if(ConfirmDelegate != null)
            {
                ConfirmDelegate(new object[] { m_input.text });
            }
            Globals.Instance.UIManager.RemoveUI(this);
        }

        private void OnClickCancel(GameObject obj, object[] args)
        {
            if(CancelDelegate != null)
            {
                CancelDelegate(args);
            }
            Globals.Instance.UIManager.RemoveUI(this);
        }
    }
}
