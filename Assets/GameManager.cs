
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Node
{
    public Node(bool _isWall, int _x, int _y) { isWall = _isWall; x = _x; y = _y; }

    public bool isWall;
    public int x, y; //현재 노드의 X,Y 좌표값
    
    public Node parentNode;

    // G : 시작으로부터 이동했던 거리
    // H : |가로|+|세로| 장애물 무시하여 목표까지의 거리, 이걸 맨해튼 거리라고 부름 
    // F : G + H, 
    
    // G는 현재 이동한 거리 , 멘헤탄 거리로 측정한 거리값의 sum값
    // H는 목표지점까지 남은 멘헤탄 거리
    public int G, H;
    public int F => G + H;
}


public class GameManager : MonoBehaviour
{
    // 기본정보
    public Vector2Int bottomLeft, topRight, startPos, targetPos; // 시작과 끝 정보
    public int scaleX = 1;
    public int scaleY = 1 ;
    private Node _startNode, _targetNode; // 시작점과 끝점 노드

    // 2D 노드맵 제작에 필요한 정보
    private Node[,] _nodeArray; // 2D 노드 맵 제작
    private int _sizeX, _sizeY; // 지도의 크기 정보
    
    // 탐색 도중 사용하는 노드
    private Node _curNode;
    private List<Node> _openList, _closedList;
    
    // 최종 경로 노드 
    public List<Node> finalNodeList;
    
    public bool allowDiagonal, dontCrossCorner;
    
    public void PathFinding()
    {
        // NodeArray의 크기 정해주고, isWall, x, y 대입
        _sizeX = topRight.x - bottomLeft.x + 1;
        _sizeY = topRight.y - bottomLeft.y + 1;

        _sizeX /= scaleX; // 임시코드
        _sizeY /= scaleY; // 임시코드, 사이즈 변경은 다음에 하는거로
        
        _nodeArray = new Node[_sizeX, _sizeY];

        // 사이즈는 스케일값으로 나눠야 제 값이 나옴
        
        // 좌하단을 [0,0]으로, 모든 타일 중심에 Collider를 탐색하는 Circle을 생성
        // Layer가 wall일 경우 true, 아닐경우 false
        // 각각의 타일의 좌표와 wall 정보를 담은 2차원 지도로 변경함
        for (int i = 0; i < _sizeX; i++)
        {
            for (int j = 0; j < _sizeY; j++)
            {
                
                bool isWall = false;
                foreach (var col in Physics2D.OverlapCircleAll(new Vector2(i + bottomLeft.x, j + bottomLeft.y), 0.4f))
                {
                    if (col.gameObject.layer == LayerMask.NameToLayer("Wall")) 
                        isWall = true;
                }
                _nodeArray[i, j] = new Node(isWall, i + bottomLeft.x, j + bottomLeft.y);
            }
        }
        
        // 시작과 목적지점 노드맵 입력
        _startNode = _nodeArray[startPos.x - bottomLeft.x, startPos.y - bottomLeft.y];
        _targetNode = _nodeArray[targetPos.x - bottomLeft.x, targetPos.y - bottomLeft.y];

        // 오픈리스트, 닫힌리스트, 마지막 리스트 초기화
        _openList = new List<Node>{ _startNode };
        _closedList = new List<Node>();
        finalNodeList = new List<Node>();


        int count = 0;
        // 여기가 탐색 코드
        while (_openList.Count > 0)
        {
            count++;
            _curNode = _openList[0];
            
            // _curNode 구하기
            // 노드DB는 _nodeArray[]
            
            // 열린 노드들에서만 검색함
            // 1. 멘헤탄 거리 + 움직인 거리가 현재보다 작거나 같음
            // 2. 멘헤탄 거리가 현재 거리보다 작음
            // 3. 이걸 만족하면 현재노드로 설정
            
            // while문이 처음 실행될 때, OpenList는 [0] 하나만 있고, 이게 시작점임
            for (int i = 1; i < _openList.Count; i++)
            {
                if (_openList[i].F <= _curNode.F 
                    && _openList[i].H < _curNode.H) 
                    _curNode = _openList[i];
            }
            
            _openList.Remove(_curNode); // 탐색하지 않은 리스트
            _closedList.Add(_curNode); // 탐색 완료된 리스트


            // 현재 노드가 목표노드까지 도달 했을 때
            // 첫번째로 목표노드에 도착한 노드가 최적화된 경로
            if (_curNode == _targetNode)
            {
                Node targetCurNode = _targetNode;
                
                // 역추적 경로
                while (targetCurNode != _startNode)
                {
                    finalNodeList.Add(targetCurNode);
                    targetCurNode = targetCurNode.parentNode;
                }
                finalNodeList.Add(_startNode);
                finalNodeList.Reverse();

                /*for (int i = 0; i < finalNodeList.Count; i++) 
                    print($"{i}번째는 {finalNodeList[i].x}, {finalNodeList[i].y}");*/
                return;
            }

            
            if (allowDiagonal)
            {
                OpenListAdd(_curNode.x + 1, _curNode.y + 1); // ↗
                OpenListAdd(_curNode.x - 1, _curNode.y + 1); // ↖
                OpenListAdd(_curNode.x - 1, _curNode.y - 1); // ↙
                OpenListAdd(_curNode.x + 1, _curNode.y - 1); // ↘
            }

            OpenListAdd(_curNode.x + 0, _curNode.y + 1); // ↑
            OpenListAdd(_curNode.x + 1, _curNode.y + 0); // →
            OpenListAdd(_curNode.x + 0, _curNode.y - 1); // ↓
            OpenListAdd(_curNode.x - 1, _curNode.y + 0); // ←

            if (count > 10000)
            {
                Debug.Log("Error");
                return;
            }
        }
    }

    void OpenListAdd(int checkX, int checkY)
    {
        int indexX = checkX - bottomLeft.x;
        int indexY = checkY - bottomLeft.y;
        int curIndexX = _curNode.x - bottomLeft.x;
        int curIndexY = _curNode.y - bottomLeft.y;
        
        
        // 상하좌우 범위를 벗어나거나
        // 벽이거나
        // 닫힌리스트에 있다면 리턴
        if ( indexX  < 0
            || indexX >= _sizeX
            || indexY < 0
            || indexY >= _sizeY
            || _nodeArray[indexX, indexY].isWall 
            || _closedList.Contains(_nodeArray[indexX, indexY])) 
            return;
        
        // 대각선 허용시, 벽 사이로 통과 안됨
        // 두 대각선 옆에 양쪽 다 벽인지 확인
        if (allowDiagonal) 
            if (_nodeArray[curIndexX, indexY].isWall 
                && _nodeArray[indexX, curIndexY].isWall) 
                return;

        // 대각선 허용 + 코너를 가로질러 가지 않을시, 이동 중에 수직수평 장애물이 있으면 안됨
        // 두 대각선 옆 한쪽이라도 뚫렸는지 확인
        if (dontCrossCorner) 
            if (_nodeArray[curIndexX, indexY].isWall 
                || _nodeArray[indexX, curIndexY].isWall) 
                return;

        // 이웃노드에 넣고, 직선은 10, 대각선은 14비용
        // 이웃은 거리가 1이니까 int로 10, 대각선은 1.414니까 int로 14
        // moveCost = 총 이동 코스트 합산
        Node neighborNode = _nodeArray[indexX, indexY];  // DB에서 이웃 노드의 값을 직접 받아옴
        
        // 현재 노드에서 이웃노드로 이동할 때의 총 비용
        int moveCost = _curNode.G + (indexX  == curIndexX || indexX  == curIndexY ? 10 : 14);
        
        // G, H, ParentNode를 설정 후 열린리스트에 추가
        if (moveCost < neighborNode.G   // 현재 이동시의 비용이 DB에 저장된 값보다 낮을경우
            || !_openList.Contains(neighborNode)) // 또는 오픈리스트에 이웃노드가 없다면 (처음으로 탐색됐다면)
        {
            // 이웃노드의 총 이동거리 갱신
            neighborNode.G = moveCost; 
            // 멘헤탄 거리 갱신 
            neighborNode.H = (Mathf.Abs(neighborNode.x - _targetNode.x) + Mathf.Abs(neighborNode.y - _targetNode.y)) * 10;

            // 이웃드의 부모님을 입력하고
            neighborNode.parentNode = _curNode;

            // 이 노드를 탐색할 노드에 추가
            _openList.Add(neighborNode);
        }
    }
    
    void OnDrawGizmos()
    {
        if(finalNodeList.Count != 0)
            for (int i = 0; i < finalNodeList.Count - 1; i++)
            {
                Gizmos.DrawLine(new Vector3(finalNodeList[i].x, finalNodeList[i].y), 
                    new Vector3(finalNodeList[i + 1].x, finalNodeList[i + 1].y));
            }
    }
} 
