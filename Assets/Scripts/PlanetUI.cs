using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class PlanetUI : MonoBehaviour
{
    public Image planetImage;
    public Image planeImage;
    public Text planetNameText;
    public Text planetInfoText;
    public Canvas canvas;
    public RectTransform canvasTransorm;
    public CelestialBody body;

    private Vector3 canvasOffset = Vector3.zero;

    public void Setup(CelestialBody celestialBody)
    {
        this.body = celestialBody;
        canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        canvasTransorm = GetComponent<RectTransform>();
        canvasTransorm.sizeDelta = new Vector2(1, 0.5f);
        canvasTransorm.anchoredPosition = new Vector2(body.transform.position.x, body.transform.localScale.y/2f + 0.25f);

        SetIcon();
        SetText();
    }

    public void SetIcon()
    {
        if (planetImage == null)
        {
            GameObject g = new GameObject("PlanetIcon", typeof(Image));
            g.transform.SetParent(this.transform);
            g.gameObject.layer = LayerMask.NameToLayer("UI");
            planetImage = g.GetComponent<Image>();
        }


        planetImage.rectTransform.sizeDelta = Vector2.one * 32;
        planetImage.transform.localScale = Vector3.one * 0.1f;
        planetImage.transform.localPosition = Vector3.zero;
        planetImage.sprite = body.planetIcon;
    }

    public void SetText()
    {
        if (planetNameText == null)
        {
            GameObject g = new GameObject("PlanetName", typeof(Text));
            g.transform.SetParent(this.transform);
            g.gameObject.layer = LayerMask.NameToLayer("UI");
            planetNameText = g.GetComponent<Text>();
        }

        RectTransform nameTextTransform = planetNameText.GetComponent<RectTransform>();
        nameTextTransform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        nameTextTransform.anchorMin = new Vector2(0, 1);
        nameTextTransform.anchorMax = new Vector2(0, 1);

        nameTextTransform.sizeDelta = new Vector2(2000, 200);
        nameTextTransform.anchoredPosition = new Vector2(1f, 0.1f);

        planetNameText.fontStyle = FontStyle.BoldAndItalic;
        planetNameText.fontSize = 175;
        planetNameText.text = body.name;
        planetNameText.color = body.orbitColor;

        if (planetInfoText == null)
        {
            GameObject g = new GameObject("PlanetInfo", typeof(Text));
            g.transform.SetParent(this.transform);
            g.gameObject.layer = LayerMask.NameToLayer("UI");
            planetInfoText = g.GetComponent<Text>();
        }

        RectTransform infoTextTransform = planetInfoText.GetComponent<RectTransform>();
        infoTextTransform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        infoTextTransform.anchorMin = new Vector2(0, 1);
        infoTextTransform.anchorMax = new Vector2(0, 1);

        infoTextTransform.sizeDelta = new Vector2(1000, 80);
        infoTextTransform.anchoredPosition = new Vector2(-50f, -10f);

        planetInfoText.fontStyle = FontStyle.Bold;
        planetInfoText.fontSize = 70;
        planetInfoText.text = "MASS Rel. to Earth: " + body.mass;
        planetInfoText.color = Color.white;
    }

    public void Show(bool show)
    {
        if (show)
        {
            canvas.enabled = true;
        }
        else
        {
            canvas.enabled = false;
        }
    }

    Vector3 CheckOverlaps()
    {
        Vector3 offset = Vector3.zero;
        if (CelestiaBodiesManager.instance != null)
        {
            foreach (var body in CelestiaBodiesManager.instance.celestialBodies)
            {
                if (body.planetUI != this)
                {
                    if (canvasTransorm.Overlaps(body.planetUI.canvasTransorm))
                    {
                        offset += new Vector3(0, body.planetUI.canvasTransorm.lossyScale.y);
                    }
                }
            }
        }
        
        return offset;
    }

    public void UpdateUI()
    {
        Vector3 planetPosition = body.reference.transform.position;
        Vector3 rocketPosition = Rocket.instance.transform.position;

        float distance = (float)Vector3.Distance(rocketPosition, planetPosition);

        //remake this pleas
        float scale = Mathf.Clamp(10 / (distance / Simulation.astronomicUnit), 1, 4);

        //and this
        if (body.radius_km * 2 > distance)
        {
            Show(false);
            return;
        }
        Show(true);

        planetImage.transform.localScale = scale * Vector3.one;

        canvasTransorm.rotation = Camera.main.transform.rotation;

        //and this
        if (WorldManager.worldType == WorldType.Rocket)
        {
            if (Camera.current)
            {
                canvasTransorm.position = Camera.current.transform.position + planetPosition.normalized * (float)((distance * Simulation.unitRocketM / Simulation.astronomicUnit) + 500);
            }
        }
        else
        {
            canvasTransorm.localPosition = new Vector2(body.transform.position.x, body.transform.localScale.y + 0.5f + body.transform.position.y);
            canvasOffset = CheckOverlaps();
            planetImage.transform.localPosition += canvasOffset;
        }
    }
}
