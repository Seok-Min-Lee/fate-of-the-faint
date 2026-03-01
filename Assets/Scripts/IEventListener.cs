public interface ICombatStarted
{
    void OnCombatStarted(CombatStarted e);
}
public interface ICombatEnded
{
    void OnCombatEnded(CombatEnded e);
}
public interface IPlayerTurnStartRequested
{
    void OnPlayerTurnStartRequested(PlayerTurnStartRequested e);
}
public interface IPlayerTurnStarted
{
    void OnPlayerTurnStarted(PlayerTurnStarted e);
}
public interface IPlayerTurnEndRequested
{
    void OnPlayerTurnEndRequested(PlayerTurnEndRequested e);
}
public interface IPlayerTurnEnded
{
    void OnPlayerTurnEnded(PlayerTurnEnded e);
}
public interface IEnemyTurnStarted
{
    void OnEnemyTurnStarted(EnemyTurnStarted e);
}
public interface IEnemyTurnEnded
{
    void OnEnemyTurnEnded(EnemyTurnEnded e);
}
public interface IEnemyActionStartRequested
{
    void OnEnemyActionStartRequested(EnemyActionStartRequested e);
}
public interface IActionStarted
{
    void OnActionStarted(ActionStarted e);
}
public interface IActionEnded
{
    void OnActionEnded(ActionEnded e);
}
public interface IEnergyChangeRequested
{
    void OnEnergyChangeRequested(EnergyChangeRequested e);
}
public interface IEnergyResolved
{
    void OnEnergyResolved(EnergyResolved e);
}
public interface IEnergyChanged
{
    void OnEnergyChanged(EnergyChanged e);
}
public interface ICardDrawed
{
    void OnCardDrawed(CardDrawed e);
}
public interface ICardDiscarded
{
    void OnCardDiscarded(CardDiscarded e);
}
public interface ICardExhausted
{
    void OnCardExhausted(CardExhausted e);
}
public interface ICardCharged
{
    void OnCardCharged(CardCharged e);
}
public interface ICardPlayDeclared
{
    void OnCardPlayDeclared(CardPlayDeclared e);
}
public interface IAttackPlayed
{
    void OnAttackPlayed(AttackPlayed e);
}
public interface ISkillPlayed
{
    void OnSkillPlayed(SkillPlayed e);
}
public interface IPowerPlayed
{
    void OnPowerPlayed(PowerPlayed e);
}
public interface IAttackDeclared
{
    void OnAttackDeclared(AttackDeclared e);
}
public interface IBlockDeclared
{
    void OnBlockDeclared(BlockDeclared e);
}
public interface IDrawCardDeclared
{
    void OnDrawCardDeclared(DrawCardDeclared e);
}
public interface IGainEnergyDeclared
{
    void OnGainEnergyDeclared(GainEnergyDeclared e);
}
public interface IModifyCostDeclared
{
    void OnModifyCostDeclared(ModifyCostDeclared e);
}
public interface IBuffDeclared
{
    void OnBuffDeclared(BuffDeclared e);
}
public interface IDamageRequested
{
    void OnDamageRequested(DamageRequested e);
}
public interface IDamageResolved
{
    void OnDamageResolved(DamageResolved e);
}
public interface IBuffRequested
{
    void OnBuffRequested(BuffRequested e);
}
public interface IBuffResolved
{
    void OnBuffResolved(BuffResolved e);
}
public interface IHpChanged
{
    void OnHpChanged(HpChanged e);
}
public interface IBlockChanged
{
    void OnBlockChanged(BlockChanged e);
}
public interface IBuffChanged
{
    void OnBuffChanged(BuffChanged e);
}
public interface IDeathDeclared
{
    void OnDeathDeclared(DeathDeclared e);
}
public interface IAnimationStarted
{
    void OnAnimationStarted(AnimationStarted e);
}
public interface IAnimationEnded
{
    void OnAnimationEnded(AnimationEnded e);
}
public interface IEnemyIntentDecided
{
    void OnEnemyIntentDecided(EnemyIntentDecided e);
}
public interface IRelicActivated
{
    void OnRelicActivated(RelicActivated e);
}