using System;
using System.Collections;
using System.Collections.Generic;
using Bot.Programming.Nodes;
using Bot.Programming.Nodes.Base;
using UnityEngine;

namespace Bot.Programming
{
    public struct ProgNodeExecutionCommand
    {
        public ProgNodeBase Node;
        public int OutputSlotIndex; // Индекс выходного слота, из которого произошел вызов
    }
    
    public class BotProgrammingController : MonoBehaviour
    {
        public event Action<ProgNodeBase> OnNodeExecutionStarted;
        public event Action<ProgNodeBase> OnNodeExecutionCompleted;
        public event Action OnExecutionCompleted;
        
        
        private BotBase _bot;
        private ProgNodeBase _entryPoint;
        private Queue<INodeExecutable> _executionQueue = new ();
        private Coroutine _executionCoroutine;
        private bool _isExecuting = false;
        private ProgNodeExecutionContext _context;
        
        // Для отладки и визуализации
        public ProgNodeBase CurrentlyExecutingNode { get; private set; }
        
        public void Init(BotBase bot, INodeExecutable entryPoint)
        {
            _bot = bot;
            //_entryPoint = entryPoint;
            _context = new ProgNodeExecutionContext { Bot = bot };
        }
        
        public void StartExecution()
        {
            if (_isExecuting)
                return;
            
            _executionQueue.Clear();
            // _executionQueue.Enqueue();
            _isExecuting = true;
            _executionCoroutine = StartCoroutine(ExecutionLoop());
        }
        
        public void StopExecution()
        {
            if (!_isExecuting)
                return;
            
            if (_executionCoroutine != null)
                StopCoroutine(_executionCoroutine);
            
            _executionQueue.Clear();
            _isExecuting = false;
            CurrentlyExecutingNode = null;
        }
        
        public void PauseExecution()
        {
            _isExecuting = false;
            if (_executionCoroutine != null)
                StopCoroutine(_executionCoroutine);
        }
        
        public void ResumeExecution()
        {
            if (_isExecuting || _executionQueue.Count == 0)
                return;
                
            _isExecuting = true;
            _executionCoroutine = StartCoroutine(ExecutionLoop());
        }
        
        public void EnqueueNode(ProgNodeBase node, int outputSlotIndex = -1)
        {
            // _executionQueue.Enqueue(new ProgNodeExecutionCommand { 
            //     Node = node, 
            //     OutputSlotIndex = outputSlotIndex 
            // });
        }
        
        private IEnumerator ExecutionLoop()
        {
            while (_isExecuting && _executionQueue.Count > 0)
            {
                // ProgNodeExecutionCommand command = _executionQueue.Dequeue();
                // CurrentlyExecutingNode = command.Node;
                //
                // OnNodeExecutionStarted?.Invoke(command.Node);
                //
                // yield return StartCoroutine(command.Node.Execute(_context, this));
                //
                // OnNodeExecutionCompleted?.Invoke(command.Node);
                
                yield return null;
            }
            
            _isExecuting = false;
            CurrentlyExecutingNode = null;
            OnExecutionCompleted?.Invoke();
        }
    }
}