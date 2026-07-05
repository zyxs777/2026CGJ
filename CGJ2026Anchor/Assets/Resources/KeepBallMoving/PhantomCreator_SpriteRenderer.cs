using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PhantomCreator_SpriteRenderer : MonoBehaviour {
    private bool _enableCreate = false;

    public bool EnableCreate {
        get {
            return _enableCreate;
        }
        set {
            if (!value) {
                for (int i = phantoms.Count - 1; i >= 0; i--) {
                    if (phantoms[i])
                        phantoms[i].color = new Color(0, 0, 0, 0);
                }
            }

            _enableCreate = value;
        }
    }

    public Transform parent;
    public float interval = 0.1f;
    public float fadeSpeed = 1.2f;
    public float startAlpha = 0.7f;
    public float endAlpha = 0.15f;

    public Color sprite_color;

    // public bool useParentScale = true;
    public Transform host;
    public SpriteRenderer source;
    public Transform scaleRoot;
    float _time = 0;
    List<SpriteRenderer> phantoms = new List<SpriteRenderer>();
    Stack<SpriteRenderer> fadePhantoms = new Stack<SpriteRenderer>();

    void FixedUpdate() {
        if (EnableCreate) {
            _time -= Time.fixedDeltaTime;
            if (_time < 0) {
                _time += interval;
                CreatePhantom();
            }

            for (int i = phantoms.Count - 1; i >= 0; i--) {
                float fadeValue = fadeSpeed * Time.deltaTime;
                phantoms[i].color -= new Color(0, 0, 0, fadeValue);
                if (phantoms[i].color.a <= endAlpha) {
                    phantoms[i].color = new Color(sprite_color.r, sprite_color.g, sprite_color.b, 0);
                    fadePhantoms.Push(phantoms[i]);
                    phantoms.RemoveAt(i);
                }
            }
        }
    }

    void CreatePhantom() {
        SpriteRenderer phantom = null;
        if (fadePhantoms.Count > 0) {
            phantom = fadePhantoms.Pop();
        } else {
            phantom = new GameObject("phantom_" + phantoms.Count).AddComponent<SpriteRenderer>();
            if (parent) {
                phantom.transform.SetParent(parent);
            }
        }

        phantom.sortingLayerName = source.sortingLayerName;
        phantom.sortingOrder = source.sortingOrder - 1;
        Vector3 tmpLocalScale = null == scaleRoot ? source.transform.parent.localScale : scaleRoot.localScale;
        phantom.transform.localScale = tmpLocalScale;
        phantom.transform.position = host.transform.position;
        phantom.transform.rotation = host.transform.parent.rotation;
        phantom.sprite = source.sprite;
        phantom.color = sprite_color;
        phantom.flipX = source.flipX;
        Color tmp_phantom_color = sprite_color;
        tmp_phantom_color.a = startAlpha;
        phantom.color = tmp_phantom_color;
        phantoms.Add(phantom);
    }

    private void OnDestroy() {
        foreach (var phantom in phantoms.Where(phantom => phantom)) {
            Destroy(phantom.gameObject);
        }

        foreach (var phantom in fadePhantoms.Where(phantom => phantom)) {
            Destroy(phantom.gameObject);
        }
    }
}