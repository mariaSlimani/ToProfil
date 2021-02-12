using System.Collections.Generic;

namespace ToProfil
{
    public static class Undo_Redo
    {
        public static Stack<DataPlan> plans_undo = new Stack<DataPlan>();
        public static Stack<DataPlan> plans_redo = new Stack<DataPlan>();
        public static void Save(Plan plan)
        {
            DataPlan planSave = new DataPlan();
            plan.TranfereData(planSave);
            plans_undo.Push(planSave);
            plans_redo.Clear();
            if (plans_undo.Count >= 2)
            {
                plan.back.IsEnabled = true;
            }
            else
            {
                plan.back.IsEnabled = false;
            }
            plan.forward.IsEnabled = false;
        } //sauvegarde du context
        public static void Undo(Plan plan)
        {
            if (plans_undo.Count <= 1) return;
            DataPlan planUndo = plans_undo.Pop();
            plans_redo.Push(planUndo.Duplicate());
            planUndo = plans_undo.Pop();
            plan.SetData(planUndo.Duplicate());
            plans_undo.Push(planUndo);
            if (plans_undo.Count >= 2)
            {
                plan.back.IsEnabled = true;
            }
            else
            {
                plan.back.IsEnabled = false;
            }
            plan.forward.IsEnabled = true;
        } //annuler le changement
        public static void Redo(Plan plan)
        {
            if (plans_redo.Count > 0)
            {
                DataPlan planRedo = plans_redo.Pop();
                plans_undo.Push(planRedo.Duplicate());
                plan.SetData(planRedo.Duplicate());
            }
            if (plans_redo.Count > 0)
            {
                plan.forward.IsEnabled = true;
            }
            else
            {
                plan.forward.IsEnabled = false;
            }
            if (plans_undo.Count >= 2)
            {
                plan.back.IsEnabled = true;
            }
            else
            {
                plan.back.IsEnabled = false;
            }
        } //retablir le changement
        public static DataPlan GetLastContexte()
        {
            return plans_undo.Peek().Duplicate();
        } //get le dernier contexte sauvegardé
    }
}
