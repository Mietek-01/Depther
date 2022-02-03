using NUnit.Framework;
using UnityEngine;

public class zones_tests
{
    [Test]
    public void each_zone_contains_start_point()
    {
        // ARRANGE
        var zones = GameObject.FindObjectsOfType<Zone>(true);

        foreach (var zone in zones)
        {
            // ACT
            var startPoint = zone.transform.Find("Others").Find("StartPoint");

            // ASSERT
            Assert.IsNotNull(startPoint);
        }
    }
}
