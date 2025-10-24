using System;
using System.Collections.Generic;
using Common.UI.Display.Overlay.Test;
using OutGame.UI.GameOver;
using UnityEngine;

namespace Common.UI.Display.Overlay
{
    [CreateAssetMenu(fileName = "OverlayRepository", menuName = "SO/UI/OverlayRepository")]
    public class OverlayRepositorySO : DisplayRepositorySOBase
    {
        protected override Dictionary<Type, Type> CreateTypeMap()
        {
            return new()
            {
                {typeof(IGameOverOverlay), typeof(GameOverOverlayLifetimeScope)},
                {typeof(ITestOverlay), typeof(TestOverlayLifeTimeScope)},
            };
        }
    }
}