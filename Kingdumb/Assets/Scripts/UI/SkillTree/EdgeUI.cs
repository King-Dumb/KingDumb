using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun.Demo.SlotRacer.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class EdgeUI : MonoBehaviour
{
    private RectTransform line;
    public RectTransform startPoint;
    public RectTransform endPoint;

    private Color activeColor = new Color(218f, 218f, 255f, 255f);

    private int fromNode;
    private int toNode;

    void Awake()
    {

        //Debug.Log("Edge UI Start");
        line = transform.GetComponent<Image>().rectTransform;
        ConnectNode();
        SetEdgeNumber();
    }

    void ConnectNode()
    {
        if (startPoint != null && endPoint != null)
        {
            // 시작점과 끝점의 중간 위치를 라인의 중심으로 설정
            Vector3 startPos = startPoint.position;
            Vector3 endPos = endPoint.position;
            Vector3 middlePos = (startPos + endPos) / 2;
            transform.position = middlePos;

            // 두 점 사이의 거리로 라인의 길이 설정
            float distance = Vector3.Distance(startPos, endPos);
            ((RectTransform)transform).sizeDelta = new Vector2(distance, line.sizeDelta.y);

            // 라인의 회전 설정
            Vector3 direction = (endPos - startPos).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    public void activeEdge()
    {
        line.gameObject.GetComponent<Image>().color = activeColor;
    }

    private void SetEdgeNumber()
    {
        string[] parts = name.Split('-');
        int.TryParse(parts[0], out int fromNode);
        int.TryParse(parts[1], out int toNode);
    }
    public int GetFromNode()
    {
        return fromNode;
    }

    public int GetToNode()
    {
        return toNode;
    }
}
