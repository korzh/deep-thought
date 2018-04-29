
/* All questions with their answers with .NET tag*/
SELECT TOP 10 P.Id as QID, P.Score as QScore, P.Title, P.Body as QBody, A.Id as AID, A.Score as AScore, A.Body as ABody
FROM Posts P
    JOIN Posts A ON P.AcceptedAnswerId = A.Id
WHERE A.PostTypeId = 2 AND P.Tags LIKE '%<.net>%'


/* all duplicated questions with .NET tag */
SELECT TOP 10 P.Id as QID, P.Score as QScore, P.Title, P.Body as QBody, PL.RelatedPostId
FROM Posts P
   JOIN PostLinks PL ON P.Id = PL.PostId AND PL.LinkTypeId = 3
WHERE A.PostTypeId = 1 AND P.Tags LIKE '%<.net>%'


