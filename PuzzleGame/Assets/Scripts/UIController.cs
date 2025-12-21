using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UIController : MonoBehaviour
{
	[SerializeField]
	private GameObject resultPanel;
	[SerializeField]
	private TextMeshProUGUI hudPlaytime;
	[SerializeField]
	private TextMeshProUGUI hudMoveCount;
	[SerializeField]
	private TextMeshProUGUI textPlaytime;
	[SerializeField]
	private TextMeshProUGUI textMoveCount;
	[SerializeField]
	private Board board;

	private static string ToMMSS(int s) => $"{s / 60:D2}:{s % 60:D2}";
	private void ApplyPlayInfo(TextMeshProUGUI tTime, TextMeshProUGUI tMoves, int sec, int moves)
	{
		tTime.text = $"PLAY TIME : {ToMMSS(sec)}";
		tMoves.text = $"MOVE COUNT : {moves}";
	}

	public void UpdateHUD()
	{
		ApplyPlayInfo(hudPlaytime, hudMoveCount, board.Playtime, board.MoveCount);
	}

	public void OnResultPanel()
	{
		resultPanel.SetActive(true);

		ApplyPlayInfo(textPlaytime, textMoveCount, board.Playtime, board.MoveCount);
	}

	public void OnClickReset()
	{
		if (resultPanel != null) resultPanel.SetActive(false);
		board.ResetGame();
	}
}