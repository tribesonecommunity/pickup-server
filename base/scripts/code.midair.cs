function RocketDumb::onCollision(%this, %owner, %clOwner, %target, %time)
{
	%objType = GetObjectType(%target);
	if(%objType == "Player")
	{
		if(!Player::ObstructionsBelow(%target, 4))
		{
			
			%clTarget = Player::GetClient(%target);
			Midair::onMidairDisc(%clOwner, %clTarget, %time);
			
		}
	}
	else if (GetObjectType(%target) == "Mine")
	{
		if (!Player::ObstructionsBelow(%owner, 4))
		{
			// Determine if it is a NJ after OnDamage by comparing getSimTime()
			// The impulse has been applied and we get player speed accurately.
			%clOwner.lastNadeCollisionTime = getSimTime();
		}
	}
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
