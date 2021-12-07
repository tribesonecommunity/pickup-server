//remember clients for 30 seconds
$Client::RememberTime = 30;

function Client::GetIP(%cl)
{
    %ip = Client::GetTransportAddress(%cl);
    
    %first = String::findsubstr(%ip, ":");
    %ip = String::getSubstr(%ip, %first+1,64);
    %second = String::findsubstr(%ip, ":");
    %ip = String::getSubstr(%ip, 0, %second);

    return %ip;
}

function Client::Remember(%cl)
{
	%ip = Client::GetIP(%cl);
	%now = getIntegerTime(true)>>5;

	$Client::Remember[ %ip, timeStamp ] = %now;
	$Client::Remember[ %ip, adminLevel ] = %cl.adminLevel;
	$Client::Remember[ %ip, registeredName ] = %cl.registeredName;
	$Client::Remember[ %ip, globalMute ] = %cl.globalMute;
	$Client::Remember[ %ip, megaMute ] = %cl.megaMute;
	
	if (($curVoteTopic != "") && ($curVoteAction == "kick") && ($curVoteOption == %cl))
		$Client::Remember[ %ip, voteKicked ] = true;
	else
		$Client::Remember[ %ip, voteKicked ] = false;
}

function Client::Recall(%cl)
{
	%ip = Client::GetIP(%cl);
	%now = getIntegerTime(true)>>5;
	
	if (%now - $Client::Remember[ %ip, timeStamp ] <= $Client::RememberTime)
	{
		if ($Client::Remember[ %ip, adminLevel ] > 0)
		{
			%cl.adminLevel = $Client::Remember[ %ip, adminLevel ];
			%cl.registeredName = $Client::Remember[ %ip, registeredName ];
			awardAdminship(%cl);

			if (%cl.canSeePlayerlist)
				LP(%cl); //spam client's console with player info

			logEntry(%cl, "had their " @ $accessLevel::[%cl.adminLevel] @ " admin restored from drop/rejoin", "", "+");
			schedule("Client::sendMessage("@%cl@", 1, \"Diggity check yo-self befo' you wreck yo'self - Your Admin was restored\");", 20);
		}

		//remember global mute
		%cl.globalMute = $Client::Remember[ %ip, globalMute ];
		%cl.megaMute = $Client::Remember[ %ip, megaMute ];

		if ($Client::Remember[ %ip, voteKicked ] && (%cl.adminLevel < 1))
			schedule("Client::DelayedVoteKick(" @ %cl @ ", \"" @ %ip @ "\");", 20);
	}
	else
	{
		deleteVariables("$Client::Remember*" @ %ip @ "*");
	}
}

function Client::DelayedVoteKick(%cl, %clientip)
{
	%ip = Client::GetIP(%cl);
	if (%ip == %clientip)
	{
		logEntry(-2, "drop/rejoin vote-kicked", %cl, "!");

		messageAll(0, "Diggity check it - " @ Client::getName(%cl) @ " was kicked for dropping during a vote to kick.");
		BanList::add(Client::getTransportAddress(%cl), 600);
		Net::kick(%cl, "Diggity check yo-self befo' you wreck yo'self - You dropped during a vote to kick you");
	}
}