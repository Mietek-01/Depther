using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using NSubstitute;
using UnityEditor;

public class player_tests
{
    [SerializeField]
    GameObject playerPrefab =
        AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Player/Player.prefab");

    [SerializeField]
    GameObject platformPrefab =
        AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Platforms/LongBlock1.prefab");

    GameObject platform;

    [UnityTest]
    public IEnumerator immortality_works_correctly()
    {
        // ARRANGE
        IPlayerInputData testInput;
        Player player = CreatePlayerWithPlatform(out testInput);

        // ACT        
        player.Immortality = true;

        yield return new WaitForSeconds(.2f);

        player.Kill(Player.KindOfDeath.BOOM);

        yield return new WaitForSeconds(.2f);

        // ASSERT       
        Assert.IsNotNull(player);
        Assert.IsFalse(player.IamDead);

        // END
        //Debug.Break();
    }

    [UnityTest]
    public IEnumerator can_not_make_dash_move_during_crouching()
    {
        // ARRANGE   
        IPlayerInputData testInput;
        Player player = CreatePlayerWithPlatform(out testInput);

        // ACT        
        testInput.Crouch.Returns(true);

        yield return new WaitForSeconds(.2f);

        testInput.UpperDoubleTap.Returns(true);

        yield return new WaitForSeconds(.2f);

        // ASSERT       
        Assert.IsFalse(player.gameObject.GetComponent<PlayerDashMoveCreator>().enabled);

        // END
        //Debug.Break();
    }

    [UnityTest]
    public IEnumerator no_crouching_during_jump()
    {
        // ARRANGE   
        IPlayerInputData testInput;
        Player player = CreatePlayerWithPlatform(out testInput);

        // ACT        
        testInput.Jump.Returns(true);

        yield return new WaitForSeconds(.1f);

        testInput.Crouch.Returns(true);

        yield return new WaitForSeconds(.2f);

        // ASSERT       
        Assert.IsFalse(player.IsCrouching);

        // END
        //Debug.Break();
    }

    [UnityTest]
    public IEnumerator no_movement_during_crouching()
    {
        // ARRANGE   
        IPlayerInputData testInput;
        Player player = CreatePlayerWithPlatform(out testInput);

        // ACT        
        testInput.Crouch.Returns(true);

        yield return new WaitForSeconds(1f);

        testInput.HorizontalMovement.Returns(1f);

        yield return new WaitForSeconds(1f);

        // ASSERT       
        Assert.IsTrue(player.IsCrouching);

        Assert.AreEqual(expected: Vector2.zero, actual: player.Velocity);

        // END
        //Debug.Break();
    }

    Player CreatePlayerWithPlatform(out IPlayerInputData testInput)
    {
        var player = UnityEngine.Object.Instantiate(playerPrefab).GetComponent<Player>();

        testInput = Substitute.For<IPlayerInputData>();
        player.AssignPlayerInputDataInterface(testInput);

        player.transform.position = Vector3.zero;

        // All tests will only need one platform
        if (platform == null)
        {
            platform = UnityEngine.Object.Instantiate(platformPrefab);
            platform.transform.position = player.transform.position - Vector3.up * 3f - Vector3.right * 3f;
        }

        return player;
    }
}
