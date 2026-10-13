namespace Scrapshift
{
    public static class CombatInput
    {
        // Reference Utils.is_manual_aim: mouse-press mode takes priority and is independent.
        public static bool ManualAim(bool always,bool onPress,bool pressed)=>onPress?pressed:always;
    }
}
