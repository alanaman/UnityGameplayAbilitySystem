using NUnit.Framework;
using UnityEngine;
using H2V.ExtensionsCore.Editor.Helpers;
using H2V.GameplayAbilitySystem.TagSystem;
using UnityEngine.TestTools;

namespace H2V.GameplayAbilitySystem.Tests.TagSystem.ScriptableObjects
{
    public class GameplayTagSOTests
    {
        private GameplayTagSO _gameplayTag;
        private GameplayTagSO _childGameplayTag;
        private GameplayTagSO _grandChildGameplayTag;

        [SetUp]
        public void Setup()
        {
            _gameplayTag = ScriptableObject.CreateInstance<GameplayTagSO>();
            _childGameplayTag = ScriptableObject.CreateInstance<GameplayTagSO>();
            _grandChildGameplayTag = ScriptableObject.CreateInstance<GameplayTagSO>();

            _childGameplayTag.SetPrivateProperty("_parent", _gameplayTag);
            _grandChildGameplayTag.SetPrivateProperty("_parent", _childGameplayTag);
        }

        [Test]
        public void IsChildTag_True()
        {
            Assert.IsTrue(_childGameplayTag.IsChildOf(_gameplayTag));
        }

        [Test]
        public void IsChildTag_SameTag_False()
        {
            Assert.IsFalse(_gameplayTag.IsChildOf(_gameplayTag));
        }

        [Test]
        public void IsGrandChildTag_Depth_1_False()
        {
            Assert.IsFalse(_grandChildGameplayTag.IsChildOf(_gameplayTag));
        }

        [Test]
        public void IsGrandChildTag_Depth_2_True()
        {
            Assert.IsTrue(_grandChildGameplayTag.IsChildOf(_gameplayTag));
        }

        [Test]
        public void SetSelfParent_Valdated_ParentNull()
        {
            LogAssert.ignoreFailingMessages = true;
            _childGameplayTag.SetPrivateProperty("_parent", _childGameplayTag);
            Assert.IsNull(_childGameplayTag.ParentTag);
        }

        [Test]
        public void SetLoopParent_Valdated_ParentNull()
        {
            LogAssert.ignoreFailingMessages = true;
            _gameplayTag.SetPrivateProperty("_parent", _grandChildGameplayTag);
            Assert.IsNull(_gameplayTag.ParentTag);
        }

        [Test]
        public void SetMaxDepthParent_Valdated_ParentNull()
        {
            LogAssert.ignoreFailingMessages = true;
            var rootTag = ScriptableObject.CreateInstance<GameplayTagSO>();
            for (int i = 0; i < 4; i++)
            {
                var childTag = ScriptableObject.CreateInstance<GameplayTagSO>();
                childTag.SetPrivateProperty("_parent", rootTag);
                rootTag = childTag;
            }
            LogAssert.ignoreFailingMessages = true;
            Assert.IsNull(rootTag.ParentTag);
        }
    }
}
