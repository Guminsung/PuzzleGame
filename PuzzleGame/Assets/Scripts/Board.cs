using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
	[SerializeField]
	private GameObject tilePrefab;                              // 숫자 타일 프리팹
	[SerializeField]
	private Transform tilesParent;                          // 타일이 배치되는 "Board" 오브젝트의 Transform

	private List<Tile> tileList;                                // 생성한 타일 정보 저장

	private Vector2Int puzzleSize = new Vector2Int(4, 4);       // 4x4 퍼즐
	private float neighborTileDistance = 102;               // 인접한 타일 사이의 거리. 별도로 계산할 수도 있다.

	public Vector3 EmptyTilePosition { set; get; }          // 빈 타일의 위치
	public int Playtime { private set; get; } = 0;      // 게임 플레이 시간
	public int MoveCount { private set; get; } = 0; // 이동 횟수

	private IEnumerator Start()
	{
		tileList = new List<Tile>();

		SpawnTiles();

		UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(tilesParent.GetComponent<RectTransform>());

		// 현재 프레임이 종료될 때까지 대기
		yield return new WaitForEndOfFrame();

		// tileList에 있는 모든 요소의 SetCorrectPosition() 메소드 호출
		tileList.ForEach(x => x.SetCorrectPosition());

		StartCoroutine("OnShuffle");
		// 게임시작과 동시에 플레이시간 초 단위 연산
		StartCoroutine("CalculatePlaytime");
	}

	private void SpawnTiles()
	{
		for (int y = 0; y < puzzleSize.y; ++y)
		{
			for (int x = 0; x < puzzleSize.x; ++x)
			{
				GameObject clone = Instantiate(tilePrefab, tilesParent);
				Tile tile = clone.GetComponent<Tile>();

				tile.Setup(this, puzzleSize.x * puzzleSize.y, y * puzzleSize.x + x + 1);

				tileList.Add(tile);
			}
		}
	}

	private IEnumerator OnShuffle()
	{
		// 1..15 + 0(빈칸)
		var arr = new List<int>(16);
		for (int i = 1; i <= 15; i++) arr.Add(i);
		arr.Add(0);

		var rng = new System.Random();
		do
		{
			// Fisher–Yates Suffle 알고리즘을 통해 랜덤하게 섞기
			for (int i = arr.Count - 1; i > 0; i--)
			{
				int j = rng.Next(i + 1);
				(arr[i], arr[j]) = (arr[j], arr[i]);
			}
		}
		while (!IsSolvable(arr, 4, 4) || IsSolved(arr)); // 불가능 및 완성 상태 제외

		// 값 순서대로 UI 자식 순서 적용(0 → 빈칸=16)
		for (int pos = 0; pos < arr.Count; pos++)
		{
			int val = (arr[pos] == 0) ? 16 : arr[pos];
			var t = tileList.Find(x => x.Numeric == val);
			t.transform.SetSiblingIndex(pos);

			yield return null;
		}

		// 빈칸 위치 저장
		var blank = tileList.Find(x => x.Numeric == 16);
		EmptyTilePosition = blank.GetComponent<RectTransform>().localPosition;
	}

	private bool IsSolved(IReadOnlyList<int> a)
	{
		for (int i = 0; i < a.Count - 1; i++)
			if (a[i] != i + 1)
				return false;
		return true;
	}
	private int CountInversions(IReadOnlyList<int> a)
	{
		var b = new List<int>(a); b.Remove(0);
		int inv = 0;
		for (int i = 0; i < b.Count; i++)
			for (int j = i + 1; j < b.Count; j++)
				if (b[i] > b[j]) inv++;
		return inv;
	}
	private bool IsSolvable(IReadOnlyList<int> a, int rows, int cols)
	{
		int inv = CountInversions(a);
		int blank = IndexOf(a, 0);
		int rowFromTop = blank / cols;
		int blankRowFromBottom = rows - rowFromTop; // 1..rows
		return (cols % 2 == 1) ? (inv % 2 == 0) : ((inv + blankRowFromBottom) % 2 == 1);
	}

	private static int IndexOf(IReadOnlyList<int> arr, int value)
	{
		for (int i = 0; i < arr.Count; i++)
			if (arr[i] == value) return i;
		return -1;
	}

	public void IsMoveTile(Tile tile)
	{
		if (Vector3.Distance(EmptyTilePosition, tile.GetComponent<RectTransform>().localPosition) == neighborTileDistance)
		{
			Vector3 goalPosition = EmptyTilePosition;

			EmptyTilePosition = tile.GetComponent<RectTransform>().localPosition;

			tile.OnMoveTo(goalPosition);

			// 타일을 이동할 때마다 이동 횟수 증가
			MoveCount++;
		}
	}

	public void IsGameOver()
	{
		List<Tile> tiles = tileList.FindAll(x => x.IsCorrected == true);

		Debug.Log("Correct Count : " + tiles.Count);
		if (tiles.Count == puzzleSize.x * puzzleSize.y - 1)
		{
			Debug.Log("GameClear");
			// 게임 클리어했을 때 시간계산 중지
			StopCoroutine("CalculatePlaytime");
			// Board 오브젝트에 컴포넌트로 설정하기 때문에
			// 그리고 한번만 호출하기 때문에 변수를 만들지 않고 바로 호출..
			GetComponent<UIController>().OnResultPanel();
		}
	}

	private IEnumerator CalculatePlaytime()
	{
		while (true)
		{
			Playtime++;

			yield return new WaitForSeconds(1);
		}
	}
}
