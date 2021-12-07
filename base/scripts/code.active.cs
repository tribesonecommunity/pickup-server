function remotezAdminActiveMode(%client)
{
	if (%client.activeMode)
		return;
	
	%client.activeMode = true;
 	
 	//active mode response to the client
	zadmin::ActiveMessage::Single(%client, zAdminActiveMode, true);

	//send score updatez for team 0/1
	zadmin::ActiveMessage::Single(%client, TeamScore, 0, $TeamScore[0]);
	zadmin::ActiveMessage::Single(%client, TeamScore, 1, $TeamScore[1]);
}

function zadmin::ActiveMessage::All(%func, %p1, %p2, %p3, %p4, %p5, %p6, %p7, %p8, %p9, %p10)
{
	for (%cl = Client::GetFirst(); (%cl != -1); %cl = Client::GetNext(%cl))
	{
		if (%cl.activeMode)
		{
			//echo("Eval " @ %func @ " for " @ %cl);
			RemoteEval(%cl, %func, %p1, %p2, %p3, %p4, %p5, %p6, %p7, %p8, %p9, %p10);
		}
	}
}

function zadmin::ActiveMessage::Single(%cl, %func, %p1, %p2, %p3, %p4, %p5, %p6, %p7, %p8, %p9, %p10)
{
	if (%cl.activeMode)
	{
		//echo("Eval " @ %func @ " for " @ %cl);
		RemoteEval(%cl, %func, %p1, %p2, %p3, %p4, %p5, %p6, %p7, %p8, %p9, %p10);
	}
}