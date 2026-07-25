using System;
using Quests;
using UI;
using UI.Notebook;
using UnityEngine;

namespace Gameplay {
    [Serializable]
    public class LevelContext {
        [field: SerializeField] public Player.Player Player { get; private set; }
        [field: SerializeField] public NotebookView NotebookView { get; private set; }
        [field: SerializeField] public QuestsService QuestsService { get; private set; }
        [field: SerializeField] public FadeScreenUI FadeScreenUI { get; private set; }
        [field: SerializeField] public Transform HouseTransform { get; private set; }
        [field: SerializeField] public Transform SpawnPosition { get; private set; }
    }
}