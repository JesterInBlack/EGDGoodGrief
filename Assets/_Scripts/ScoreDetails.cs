public class SubScore
{
	//A lightweight class for storing data for a particular score objective
	public float score; //points for the objective
	public float count; //associated numerical data: usually # of times
	public string name; //name of the objective

	public SubScore ()
	{
		name = "";
		score = 0;
		count = 0;
	}

	public SubScore ( string pName )
	{
		name = pName;
		score = 0;
		count = 0;
	}
}

public class ScoreDetails
{
	//A class for storing detailed score data
	#region vars
	public SubScore downs;
	public SubScore damageDealt;
	public SubScore damageTaken;
	public SubScore damageAvoided;
	public SubScore lastHit;
	public SubScore misc;
	//public SubScore revives;
	//public SubScore griefs;
	//public SubScore unity;
	//public SubScore dissension;
	#endregion

	public ScoreDetails()
	{
		downs = new SubScore( "Downs" );
		damageDealt = new SubScore( "Damage Dealt" );
		damageTaken = new SubScore( "Damage Taken" );
		damageAvoided = new SubScore( "Dodge / Block" );
		lastHit = new SubScore( "Boss Killer" );
		misc = new SubScore( "Objective Bonus" );
	}

	public float GetGrandTotal()
	{
		return downs.score + damageDealt.score + damageTaken.score + damageAvoided.score + lastHit.score;
	}

	public SubScore[] GetObjectiveScores()
	{
		//iterate through each subscore, and return its score.
		//TODO: return subscore object
		SubScore[] objectiveScores = new SubScore[6];

		objectiveScores[0] = damageDealt;
		objectiveScores[1] = damageAvoided;
		objectiveScores[2] = damageTaken;
		objectiveScores[3] = downs;
		objectiveScores[4] = misc;
		objectiveScores[5] = lastHit;

		return objectiveScores;
	}
}
