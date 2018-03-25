using UnityEngine;
using UnityEngine.EventSystems;

namespace Osblow
{
    public class CustomEventTrigger : EventTrigger
    {
        public delegate void Callback(GameObject obj, object[] args);

        public object[] Args;


        public event Callback Callback_OnBeginDrag;
        public event Callback Callback_OnDrag;
        public event Callback Callback_OnEndDrag;

        public event Callback Callback_OnClick;


        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);
            if(Callback_OnBeginDrag != null)
            {
                Callback_OnBeginDrag(gameObject, Args);
            }
        }

        public override void OnCancel(BaseEventData eventData)
        {
            base.OnCancel(eventData);
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            base.OnDrag(eventData);

            if (Callback_OnDrag != null)
            {
                Callback_OnDrag(gameObject, Args);
            }
        }

        public override void OnDrop(PointerEventData eventData)
        {
            base.OnDrop(eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);

            if (Callback_OnEndDrag != null)
            {
                Callback_OnEndDrag(gameObject, Args);
            }
        }

        public override void OnInitializePotentialDrag(PointerEventData eventData)
        {
            base.OnInitializePotentialDrag(eventData);
        }

        public override void OnMove(AxisEventData eventData)
        {
            base.OnMove(eventData);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);

            if(Callback_OnClick != null)
            {
                Callback_OnClick(gameObject, Args);
            }
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
        }

        public override void OnScroll(PointerEventData eventData)
        {
            base.OnScroll(eventData);
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
        }

        public override void OnSubmit(BaseEventData eventData)
        {
            base.OnSubmit(eventData);
        }

        public override void OnUpdateSelected(BaseEventData eventData)
        {
            base.OnUpdateSelected(eventData);
        }
    }
}
