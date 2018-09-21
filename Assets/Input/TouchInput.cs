using UnityEngine;
using UnityEngine.EventSystems;

public class TouchInput : MonoBehaviour, IDragHandler {

	public static System.Action<Vector2> OnDragEvent = v => { };

	public void OnDrag(PointerEventData eventData) {
		OnDragEvent.Invoke(eventData.delta);
		Debug.LogFormat("[TouchInput] drag: {0}", eventData.delta);
	}
}