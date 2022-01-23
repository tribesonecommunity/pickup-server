function RocketDumb::onCollision(%this, %owner, %clOwner, %target, %time)
{
    %objType = GetObjectType(%target);
    if(%objType == "Player")
    {
        
        //prevent a rare situation
        %clTarget = Player::GetClient(%target);
        if (%clTarget == -1) { return; }
        
        if(!Player::ObstructionsBelow(%target, $Game::Midair::Height))
        {
            Midair::onMidairDisc(%clOwner, %clTarget, %time);
        }
    }
    else if (%objType == "Mine")
    {
        if (!Player::ObstructionsBelow(%owner, $Game::Midair::Height))
        {
            // Determine if it is a NJ after OnDamage by comparing getSimTime()
            // The impulse has been applied and we get player speed accurately.
            %clOwner.lastNadeCollisionTime = getSimTime();
        }
    }
    else { }
}

function Player::ObstructionsBelow(%pl, %distance)
{
    %armor = Player::getArmor(%pl);
    %pos = GameBase::getPosition(%pl);
    %height = "0 0 " @ -%armor.boxNormalHeight;
    while(%distance > 0)
    {
        %pos = Vector::Add(%pos, %height);
        if(!GameBase::testPosition(%pl, %pos))
            return(true);
        %distance -= %armor.boxNormalHeight;
    }
    return (false);
}

