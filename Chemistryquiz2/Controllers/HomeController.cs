using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Chemistryquiz2.Models;

namespace Chemistryquiz2.Controllers
{
    public class HomeController : Controller
    {
        private MyQuizEntities _entitites = new MyQuizEntities();

        // GET: Quiz

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Quiz(int quizId)
        {
            Quizze quizToDisplay = _entitites.Quizzes.Find(quizId);

            return View(quizToDisplay);
        }


        [HttpPost]
        public ActionResult Quiz(FormCollection form)
        {
            int formLength = form.Count;
            string quizName = (string)form[formLength - 1];
            int quizLength = formLength - 1;
            int[] quizAnswers = new int[quizLength];
            int quizId = 0;

            //get quiz answers from form and put them into an array
            for (int i = 0; i < quizLength; i++)
            {
                quizAnswers[i] = Convert.ToInt32(form[i]);
            }
            TempData["quizAnswers"] = quizAnswers;
            TempData["quizLength"] = quizLength;

            //call appropriate quiz scoring method
            if (quizName == "Submit Chemistry Quiz")
            {
                ScoreChemistryQuiz();
                int resultId = (int)TempData["resultId"];
                //send quiz id, so that results feedback can be adjusted
                quizId = 1;
                TempData["quizId"] = quizId;
                return RedirectToAction(actionName: "Result", routeValues: new { Id = resultId });
            }
            else return null;
        }

        public void ScoreChemistryQuiz()
        {
            int quizLength = (int)TempData["quizLength"];
            int[] quizAnswers = new int[quizLength];
            int quizScore = 0;

            quizAnswers = (int[])TempData["quizAnswers"];

            // check quiz answers and work out the score
            for (int i = 0; i < quizLength; i++)
            {
                int currentQuizAnswer = quizAnswers[i];

                //check answer against DB
                var checkedAnswer = (from answer in _entitites.Answers
                                     where answer.Id == currentQuizAnswer
                                     select answer.Correct).Single();

                bool answerCorrect = Convert.ToBoolean(checkedAnswer);

                if (answerCorrect)
                {
                    quizScore++;
                }
            }

            int resultId = 0;

            //determine which results content will be retrieved
            if (quizScore < 9)
            {
                resultId = 1;
            }

            else
            {
                resultId = 14;
            }

            TempData["resultId"] = resultId;
            TempData["quizScore"] = quizScore;
        }

        public PartialViewResult _Questions(int quizId)
        {
            ViewBag.QuizId = quizId;

            var questionList = from question in _entitites.Questions
                               where question.QuizId == quizId
                               select question;

            return PartialView(questionList);
        }

        public PartialViewResult _Answers(int questionId)
        {
            var answerList = from answer in _entitites.Answers
                             where answer.QuestionID == questionId
                             select answer;

            return PartialView(answerList);
        }
        public ActionResult Result()
        {
            int resultId = (int)TempData["resultId"];
            int quizId = (int)TempData["quizId"];
            //display score only for Houses Quiz
            if (quizId == 1)
            {
                int quizScore = (int)TempData["quizScore"];
                ViewBag.QuizScore = quizScore;
            }
            var correctResult = (from result in _entitites.Results
                                 where result.Id == resultId
                                 select result).Single();

            return View(correctResult);
        }
    }
}