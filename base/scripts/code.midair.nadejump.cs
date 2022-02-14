Attachment::AddAfter("Player::onDamage", "Midair::NadeJump::AfterOnDamage");
// Called post so we don't announce on suicide nade jumps

function Midair::NadeJump::AfterOnDamage(%this, %type, %value, %pos, %vec, %mom, %vertPos, %quadrant, %object)
{
	//damage from impact etc.
	if(%type == 0)
		return;

	%time = getSimTime();

	%shooterClient = %object;
	%damagedClient = Player::getClient(%this);

	// Team damage
	if (%type == $ExplosionDamageType) {	// Self-damage, check for nade jump
		if (%shooterClient == %damagedClient) {
		 	if (%time == %shooterClient.lastNadeCollisionTime) {
				Event::AnnounceNadeJump(%shooterClient);
			}
		}
	}
}

function Event::AnnounceNadeJump(%cl)
{
	Client::adjustScore(%cl, "NadeJump");
	%vel = Item::GetVelocity(Client::GetControlObject(%cl));
	%speed = Vector::GetDistance("0 0 0", %vel);
	Client::SendMessage(%cl, 0, "~wmine_act.wav");
	zadmin::ActiveMessage::All(onNadeJump, %cl, %speed);
    //
    Collector::onNadeJump( %cl, %speed );
}

