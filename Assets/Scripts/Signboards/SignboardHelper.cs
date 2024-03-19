
public static class SignboardHelper {
    public static IFCSignBoard[] GetIFCSignboards() {
        return Utility.GetComponentsFromTag<IFCSignBoard>(Constants.SIGNBOARDS_TAG);
    }
    
    
    public static SignboardVisibility[] GetSignboardVisibilities() {
        return Utility.GetComponentsFromTag<SignboardVisibility>(Constants.SIGNBOARDS_TAG);
    }
}