function zadmin::AFKDaemon()
{
	if (!$zadmin::pref::afk::enabled)
		return;
	
	$zadmin::AFKDaemonTimestamp = getSimTime();

	if (!$Server::TourneyMode)
	{
		%now = getSimTime();

		%floor = ($accessLevel::Count-1);
		if (%floor<1)
			%floor = 1;

		for(%cl = Client::getFirst(); %cl != -1; %cl = Client::getNext(%cl))
		{
			//echo(Client::GetName(%cl) @ ", " @ %now - %cl.lastActiveTimestamp);
			if ((%now - %cl.lastActiveTimestamp) >= $zadmin::pref::afk::timelimit)
			{
				%exempt = false;
				%exempt = %exempt || (%cl.adminLevel >= %floor);
				%exempt = %exempt || ((%cl.adminLevel > 0) && (Client::GetTeam(%cl) == -1));
				
				if (!%exempt)
					Net::kick(%cl, "If you are going to go AFK, please exit the server.");
			}
		}
	}
	
	schedule("zadmin::AFKDaemon();", $zadmin::pref::afk::monitorInterval);
}

function zadmin::AFKStatus()
{
	echo("Next AFK Check in: " @ $zadmin::pref::afk::monitorInterval - (getSimTime() - $zadmin::AFKDaemonTimestamp) @ " seconds ");
	
	if (!$Server::TourneyMode)
	{
		%now = getSimTime();
		for(%cl = Client::getFirst(); %cl != -1; %cl = Client::getNext(%cl))
		{
			%time = (%now - %cl.lastActiveTimestamp);
			echo(client::getname(%cl) @ " idle time: " @ %time);
		}
	}
}