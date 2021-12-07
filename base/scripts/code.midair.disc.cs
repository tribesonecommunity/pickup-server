function Midair::onMidairDisc(%clOwner, %clTarget, %time)
{
	
	%teammateMA = 0;
	%ownerTeam = GameBase::getTeam(%clOwner);
	%targetTeam = GameBase::getTeam(%clTarget);
	
	if (%ownerTeam == %targetTeam) { %teammateMA = 1; }

	%meters = floor(%time * 0.065);
	
	if (%meters >= 50 && %teammateMA == 0) {
		Client::adjustScore(%clOwner, "MidAirLong");
		Client::adjustScore(%clTarget, "MidAirCatch");
		MessageAll(0, Client::GetName(%clOwner) @ " lands [ " @ %meters @ " meter ] mid-air on " @ Client::GetName(%clTarget) @ "!");
		Client::SendMessage(%clOwner, 0, "~wc_buysell.wav");
		
	}
	else if (%teammateMA == 0) {
		Client::adjustScore(%clOwner, "MidAir");
		Client::SendMessage(%clOwner, 0, "You just hit a [ " @ %meters @ " meter ] mid-air on " @ Client::GetName(%clTarget) @ "!~wc_buysell.wav");
	}
	else {
		Client::SendMessage(%clOwner, 0, "You just hit a [ " @ %meters @ " meter ] mid-air on your teammate " @ Client::GetName(%clTarget) @ "!~wc_buysell.wav");
	}
		
	zadmin::ActiveMessage::All( MidairDisc, Client::GetName(%clOwner), Client::GetName(%clTarget), %time );
}

