public class SubScore
{
	//A lightweight class for storing data for a particular score objective
	public float score; //points for the objective
	public float count; //associated numerical data: usually # of times

	public SubScore ()
	{
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
	//public SubScore revives;
	//public SubScore griefs;
	//public SubScore unity;
	//public SubScore dissension;
	#endregion

	public ScoreDetails()
	{
		downs = new SubScore();
		damageDealt = new SubScore();
		damageTaken = new SubScore();
		damageAvoided = new SubScore();
		lastHit = new SubScore();
	}

	public float GetGrandTotal()
	{
		return downs.score + damageDealt.score + damageTaken.score + damageAvoided.score + lastHit.score;
	}

	public float[] GetObjectiveScores()
	{
		//iterate through each subscore, and return its score.
		float[] objectiveScores = new float[5];

		objectiveScores[0] = damageDealt.score;
		objectiveScores[1] = damageAvoided.score;
		objectiveScores[2] = damageTaken.score;
		objectiveScores[3] = downs.score;
		objectiveScores[4] = lastHit.score;

		return objectiveScores;
	}
}
