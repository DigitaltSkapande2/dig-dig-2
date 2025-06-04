using UnityEngine;

public class GameInputManager : Singleton<GameInputManager>
{
    public GameInputSystem gameInputSystem;

	protected override void Awake()
    {
        base.Awake();

        gameInputSystem = new();
    }
}
