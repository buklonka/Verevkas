using UnityEngine;

public class RopeSegment : MonoBehaviour
{
    public DynamicRope parentRope;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        RopeSegment otherSegment = collision.gameObject.GetComponent<RopeSegment>();
        if (otherSegment != null)
        {
            // Check if the collision is with a segment from a different rope
            if (otherSegment.parentRope != this.parentRope)
            {
                parentRope.AddIntersection(otherSegment.parentRope);
                otherSegment.parentRope.AddIntersection(this.parentRope);

                // Play collision sound if the impact is significant enough
                if (collision.relativeVelocity.magnitude > 1.0f)
                {
                    SoundManager.Instance.PlayRopeCollision();
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        RopeSegment otherSegment = collision.gameObject.GetComponent<RopeSegment>();
        if (otherSegment != null)
        {
            if (otherSegment.parentRope != this.parentRope)
            {
                parentRope.RemoveIntersection(otherSegment.parentRope);
                otherSegment.parentRope.RemoveIntersection(this.parentRope);
            }
        }
    }
}
