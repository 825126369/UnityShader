using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

using LuaInterface;

public class UGUIPlaySound : MonoBehaviour, IMoveHandler,
        IPointerDownHandler, IPointerUpHandler,
        IPointerEnterHandler, IPointerExitHandler,
        ISelectHandler, IDeselectHandler, IPointerClickHandler,
        ISubmitHandler, ICancelHandler
{
    public enum Trigger
    {
        None = 0,       //不执行
        OnClick,
        OnDown,
        OnUp,
        Custom,
        OnEnable,   //界面隐藏
        OnDisable,
        OnDestroy, //界面关闭
    }

    public string audioName = "";
    public AudioClip audioClip = null;
    public Trigger trigger = Trigger.OnClick;

    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(0f, 2f)]
    public float pitch = 1f;

    bool mIsOver = false;

    private Selectable mCurSelectableComponent = null;

    bool canPlay
    {
        get
        {
            if (!enabled) return false;
            //if (mCurSelectableComponent == null)
            //    mCurSelectableComponent = GetComponent<Selectable>();
            return ! (this.mCurSelectableComponent == null || !this.mCurSelectableComponent.interactable
            || !this.mCurSelectableComponent.enabled);
        }
    }

    void Awake()
    {
        mCurSelectableComponent = GetComponent<Selectable>();
        if (mCurSelectableComponent == null)
        {
            mCurSelectableComponent = gameObject.AddComponent<Selectable>();
            mCurSelectableComponent.transition = Selectable.Transition.None;
        }
    }

#if UNITY_EDITOR
    //void Reset()
    //{
    //    Debug.Log("add Scripts");
    //}
    void OnValidate()
    {
        if (audioClip)
        {
            audioName = audioClip.name;
            audioClip = null;
        }
    }
#endif

    void TryPlaySound()
    {
        if (LuaManager.Instance.Lua != null)
        {
            if (audioName == "")
            {
                audioName = "sound_dianji";
            }
            LuaFunction lf = LuaManager.Instance.GetFunction("PlayUISound");
            lf.Call(audioName);
            lf.Dispose();
            lf = null;
        }
        //else
        //{
        //    AudioClip clip = Resources.Load<AudioClip>("sound_dianji");
        //    if (clip != null)
        //        NGUITools.PlaySound(clip, volume, pitch);
        //}
    }

    void OnEnable()
    {
        if (trigger == Trigger.OnEnable)
            TryPlaySound();
    }

    void OnDisable()
    {
        if (trigger == Trigger.OnDisable)
            TryPlaySound();
    }

    void OnDestroy()
    {
        if (trigger == Trigger.OnDestroy)
            TryPlaySound();
    }

    /// <summary>
    /// lua bind method
    /// </summary>
    public void Play()
    {
        TryPlaySound();
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (canPlay && trigger == Trigger.OnClick)
            TryPlaySound();
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (canPlay && trigger == Trigger.OnDown)
            TryPlaySound();

    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        if (canPlay && trigger == Trigger.OnUp)
            TryPlaySound();
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {

    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {

    }

    public virtual void OnSelect(BaseEventData eventData)
    {

    }

    public void OnDeselect(BaseEventData eventData)
    {

    }

    public virtual void OnMove(AxisEventData eventData)
    {

    }

    public void OnSubmit(BaseEventData eventData)
    {
       
    }

    public void OnCancel(BaseEventData eventData)
    {
       
    }
}
