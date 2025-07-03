using NUnit.Framework;
using UnityEngine;
using H2V.GameplayAbilitySystem.AbilitySystem.Components;
using H2V.GameplayAbilitySystem.TagSystem;
using UnityEditor;
using H2V.ExtensionsCore.Editor.Helpers;
using H2V.GameplayAbilitySystem.AbilitySystem;

namespace H2V.GameplayAbilitySystem.Tests.AbilitySystem
{
    public class AbilitySpecTests
    {
        private AbilitySystemBehaviour _abilitySystem;
        private AbilitySystemBehaviour _otherAbilitySystem;
        private TestAbility _testAbility;
        private TestAbility _otherTestAbility;
        private GameplayTagSO _requiredGameplayTag;
        private GameplayTagSO _ignoreGameplayTag;
        private GameplayTagSO _activationGameplayTag;
        private GameplayTagSO _blockGameplayTag;
        private GameplayTagSO _cancelGameplayTag;

        [SetUp]
        public void SetUp()
        {
            var go = new GameObject();
            _abilitySystem = go.AddComponent<AbilitySystemBehaviour>();
            var otherGo = new GameObject();
            _otherAbilitySystem = otherGo.AddComponent<AbilitySystemBehaviour>();

            _testAbility = ScriptableObject.CreateInstance<TestAbility>();
            _otherTestAbility = ScriptableObject.CreateInstance<TestAbility>();

            _requiredGameplayTag = ScriptableObject.CreateInstance<GameplayTagSO>();
            _ignoreGameplayTag = ScriptableObject.CreateInstance<GameplayTagSO>();
            _activationGameplayTag = ScriptableObject.CreateInstance<GameplayTagSO>();
            _blockGameplayTag = ScriptableObject.CreateInstance<GameplayTagSO>();
            _cancelGameplayTag = ScriptableObject.CreateInstance<GameplayTagSO>();
        }

        private void AddTagToList(ref GameplayTagSO[] listToAdd, params GameplayTagSO[] tags)
        {
            ArrayUtility.AddRange(ref listToAdd, tags);
        }

        private void RemoveTagFromList(ref GameplayTagSO[] listToAdd, params GameplayTagSO[] tags)
        {
            foreach (var tag in tags)
            {
                var index = ArrayUtility.FindIndex(listToAdd, t => t == tag);
                if (index != -1)
                ArrayUtility.RemoveAt(ref listToAdd, index);
            }
        }

        [Test]
        public void InitAbility_CorrectlyInit()
        {
            var abilitySpec = _abilitySystem.GiveAbility<TestAbilitySpec>(_testAbility);

            Assert.AreEqual(_testAbility, abilitySpec.AbilityDef);
            Assert.AreEqual(_abilitySystem, abilitySpec.Owner);
            Assert.AreEqual(_abilitySystem, abilitySpec.Source);
            Assert.AreEqual(0, abilitySpec.Targets.Count);
        }

        [Test]
        public void CanActiveAbility_Activated_ReturnsFalse()
        {
            var abilitySpec = _abilitySystem.GiveAbility<TestAbilitySpec>(_testAbility);
            _abilitySystem.TryActiveAbility(abilitySpec);
            Assert.IsFalse(abilitySpec.CanActiveAbility());
        }

        [Test]
        public void CanActiveAbility_IsNotActive_ReturnsTrue()
        {
            var abilitySpec = _abilitySystem.GiveAbility<TestAbilitySpec>(_testAbility);
            Assert.IsTrue(abilitySpec.CanActiveAbility());
        }

        [Test]
        public void CanActiveAbility_InvalidOwner_ReturnsFalse()
        {
            var abilitySpec = _abilitySystem.GiveAbility<TestAbilitySpec>(_testAbility);
            _abilitySystem.gameObject.SetActive(false);
            Assert.IsFalse(abilitySpec.CanActiveAbility());

            _abilitySystem.gameObject.SetActive(true);
            GameObject.DestroyImmediate(_abilitySystem.gameObject);
            Assert.IsFalse(abilitySpec.CanActiveAbility());
        }

        [Test]
        public void CanActiveAbility_DoesOwnerSatisfyTagRequirements()
        {
            // In this test Source is the same as Owner so I only test Owner
            var abilitySpec = _abilitySystem.GiveAbility<TestAbilitySpec>(_testAbility);
            AddTagToList(ref _testAbility.Tags.OwnerTags.RequireTags, _requiredGameplayTag);
            Assert.IsFalse(abilitySpec.CanActiveAbility());
            _abilitySystem.GameplayGameplayTags.AddTags(_requiredGameplayTag);
            Assert.IsTrue(abilitySpec.CanActiveAbility());

            AddTagToList(ref _testAbility.Tags.OwnerTags.IgnoreTags, _ignoreGameplayTag);
            _abilitySystem.GameplayGameplayTags.AddTags(_ignoreGameplayTag);
            Assert.IsFalse(abilitySpec.CanActiveAbility());
            _abilitySystem.GameplayGameplayTags.RemoveTags(_ignoreGameplayTag);
            Assert.IsTrue(abilitySpec.CanActiveAbility());
        }

        [Test]
        public void CanActiveAbility_IsPassAllCondition()
        {
            var abilitySpec = _abilitySystem.GiveAbility<TestAbilitySpec>(_testAbility);
            _testAbility.SetPrivateArrayProperty("Conditions", new IAbilityCondition[] { new AlwaysFalse() }, true);
            Assert.IsFalse(abilitySpec.CanActiveAbility());
            _testAbility.SetPrivateArrayProperty("Conditions", new IAbilityCondition[] { new AlwaysTrue() }, true);
            Assert.IsTrue(abilitySpec.CanActiveAbility());
            _testAbility.SetPrivateArrayProperty("Conditions", 
                new IAbilityCondition[] { new AlwaysTrue(), new AlwaysFalse() }, true);
            Assert.IsFalse(abilitySpec.CanActiveAbility());
        }

        [Test]
        public void CanActiveAbility_IsBlockedByOtherAbility()
        {
            AddTagToList(ref _otherTestAbility.Tags.BlockAbilityWithTags, _blockGameplayTag);
            _testAbility.Tags.abilityGameplayTag = _blockGameplayTag;

            var abilitySpec = _abilitySystem.GiveAbility<TestAbilitySpec>(_testAbility);
            var blockAbilitySpec = _abilitySystem.GiveAbility<TestAbilitySpec>(_otherTestAbility);

            _abilitySystem.TryActiveAbility(blockAbilitySpec);
            // Blocked by other ability in system
            Assert.IsFalse(_abilitySystem.TryActiveAbility(abilitySpec));
        }

        [Test]
        public void ActivateAbility_AbilityActive_SystemHasActivationTags()
        {
            var abilitySpec = _abilitySystem.GiveAbility<TestAbilitySpec>(_testAbility);
            AddTagToList(ref _testAbility.Tags.ActivationTags, _activationGameplayTag);
            _abilitySystem.TryActiveAbility(abilitySpec);
            Assert.IsTrue(abilitySpec.IsActive);

            Assert.IsTrue(_abilitySystem.GameplayGameplayTags.TagSet.HasTag(_activationGameplayTag));
        }

        [Test]
        public void ActivateAbility_RemoveInappropriateTargets_TargetSatisfyTagRequirements()
        {

            var abilitySpec = _abilitySystem.GiveAbility<TestAbilitySpec>(_testAbility);
            AddTagToList(ref _testAbility.Tags.TargetTags.RequireTags, _requiredGameplayTag);

            _abilitySystem.TryActiveAbility(abilitySpec, _otherAbilitySystem);
            Assert.AreEqual(0, abilitySpec.Targets.Count);

            _otherAbilitySystem.GameplayGameplayTags.AddTags(_requiredGameplayTag);

            _abilitySystem.TryActiveAbility(abilitySpec, _otherAbilitySystem);

            Assert.AreEqual(1, abilitySpec.Targets.Count);
        }

        [Test]
        public void ActivateAbility_CancelTargetsAbilities()
        {
            _otherTestAbility.Tags.abilityGameplayTag = _cancelGameplayTag;
            AddTagToList(ref _testAbility.Tags.CancelAbilityWithTags, _cancelGameplayTag);

            var otherAbilitySpec = _otherAbilitySystem.GiveAbility<TestAbilitySpec>(_otherTestAbility);
            _otherAbilitySystem.TryActiveAbility(otherAbilitySpec);

            var abilitySpec = _abilitySystem.GiveAbility<TestAbilitySpec>(_testAbility);
            _abilitySystem.TryActiveAbility(abilitySpec, _otherAbilitySystem);

            Assert.IsFalse(otherAbilitySpec.IsActive);
        }

        [Test]
        public void EndAbility_AbilityNotActive_SystemHasNoTags()
        {
            ActivateAbility_AbilityActive_SystemHasActivationTags();
            var abilitySpec = _abilitySystem.GrantedAbilities[0];
            abilitySpec.EndAbility();
            Assert.IsFalse(abilitySpec.IsActive);
            Assert.IsFalse(_abilitySystem.GameplayGameplayTags.TagSet.HasTag(_activationGameplayTag));
        }


        [Test]
        public void InitAbility_CorrectlyContext()
        {
            _testAbility.SetPrivateArrayProperty("Contexts", new IAbilityContext[] { new TestContext() }, true);
            _abilitySystem.GiveAbility<TestAbilitySpec>(_testAbility);
        }
    }
}