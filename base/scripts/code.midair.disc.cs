function Midair::onMidairDisc(%clOwner, %clTarget, %time)
{
    %ownerTeam = GameBase::getTeam(%clOwner);
    %targetTeam = GameBase::getTeam(%clTarget);
    
    %teammateMA = 0;
    if (%ownerTeam == %targetTeam) { %teammateMA = 1; }

    %meters = floor(%time * 0.065);
    if (%teammateMA == 0) {
        if (%meters >= 50) {
            Client::adjustScore(%clOwner, "MidAirLong");
            Client::adjustScore(%clTarget, "MidAirLongCatch");
            MessageAll(0, Client::GetName(%clOwner) @ " lands [ " @ %meters @ " meter ] mid-air on " @ Client::GetName(%clTarget) @ "!");
            Client::SendMessage(%clOwner, 0, "~wc_buysell.wav");
        }
        else {
            Client::adjustScore(%clOwner, "MidAir");
            Client::SendMessage(%clOwner, 0, "You just hit a [ " @ %meters @ " meter ] mid-air on " @ Client::GetName(%clTarget) @ "!~wc_buysell.wav");
        }
        zadmin::ActiveMessage::All( MidAirDisc, %clOwner, %clTarget, %time ); 
    }
    else {
        Client::SendMessage(%clOwner, 0, "You just hit a [ " @ %meters @ " meter ] mid-air on your teammate " @ Client::GetName(%clTarget) @ "!~wc_buysell.wav");
    }
}

