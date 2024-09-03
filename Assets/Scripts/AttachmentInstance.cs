public class AttachmentInstance : BaseItemInstance
{
    public AttachmentInstance(AttachmentItemData itemData, int x, int y) : base(itemData, x, y)
    {
    }

    // Attachments don't have a right-click action, so we don't need to implement any additional methods
}