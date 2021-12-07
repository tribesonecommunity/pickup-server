exec("code.ratings.cs");

//adjust the client score by [activity], optionally 
//passing a percent value to adjust the added score by
function Client::adjustScore(%cl, %activity)
{
	%score = $Stats::Rating[%activity];

	if ((%score == 0) || (%score == ""))
		return;
	
	%cl.score += %score;
	
	%team = Client::getTeam(%cl);
	if(%team == -1) // observers go last.
		%team = 9;
   
	// objective mission sorts by team first.
	Client::setScore(%cl, "%n\t%t\t  " @ floor(%cl.score)  @ "\t%p\t%l", %cl.score + (9 - %team) * 10000);
}

function Client::refreshScore(%cl)
{
	%team = Client::getTeam(%cl);
	if(%team == -1) // observers go last.
		%team = 9;
   
	// objective mission sorts by team first.
	Client::setScore(%cl, "%n\t%t\t  " @ floor(%cl.score)  @ "\t%p\t%l", %cl.score + (9 - %team) * 10000);
}

function Client::adjustScoreNoUpdate(%cl, %activity, %pct)
{
	%score = $Stats::Rating[%activity];
	if (%pct)
		%score = %score * %pct;
	
	if ((%score == 0) || (%score == ""))
		return;
	
	%cl.score += %score;
	
	//Game::refreshClientScore(%cl);
}

//manage flag grabs
function Client::onFlagGrab(%team, %cl)
{
	$Stats::FlagLoc[%team] = %cl;
	
	$Stats::GrabList[%team, 0] = %cl;
	$Stats::GrabList[%team, 0, name] = Client::GetName(%cl);
	$Stats::GrabList[%team, 1] = "";
	$Stats::GrabTime[%team] = getSimTime();

	Client::adjustScore(%cl, "Grab");
}

//manage flag pickups in the field
function Client::onFlagPickup(%team, %cl)
{
	$Stats::FlagLoc[%team] = %cl;
	
	%found = false;
	for (%i=0; $Stats::GrabList[%team, %i] != ""; %i++)
	{
		if ($Stats::GrabList[%team, %i] == %cl)
			%found = true;
	}
	
	if (!%found)
	{
		$Stats::GrabList[%team, %i] = %cl;
		$Stats::GrabList[%team, %i, name] = Client::GetName(%cl);
		$Stats::GrabList[%team, %i + 1] = "";
	}
	
	Client::adjustScore(%cl, "Pickup");
}


//manage flag drops
function Client::onFlagDrop(%team, %cl)
{
	$Stats::FlagLoc[%team] = "field";
	Client::adjustScore(%cl, "Drop");
}

//manage flag returns
function Client::onFlagReturn(%team, %cl)
{
	//find enemy team
	if (%team == 0) {
		%enemyTeam = 1;
	}
	else {
		%enemyTeam = 0;
	}
	
	$Stats::FlagLoc[%team] = "home";
	
	if (!%cl) {
		//if no client
		return;
	}
	if (((getSimTime() - $Stats::GrabTime[%team] > 30) && ($Stats::FlagLoc[%enemyTeam] != "home")) || (getSimTime() - $Stats::GrabTime[%team] > 90)) {
		//if standoff return, reward more points
		Client::adjustScore(%cl, "StandoffReturn");
	}
	else {
		//standard return if above condition not met
		Client::adjustScore(%cl, "Return");
	}
}

//manage flag caps
function Client::onFlagCap(%team, %cl)
{
	$Stats::FlagLoc[%team] = "home";
	
	Client::adjustScore(%cl, "Cap");
	
	for (%i=0; (%assistcl = $Stats::GrabList[%team, %i]) != ""; %i++)
	{
		if (%assistcl != %cl)
			if ($Stats::GrabList[%team, %i, name] == Client::GetName(%assistcl))
				Client::adjustScore(%assistcl, "Assist");
	}
}