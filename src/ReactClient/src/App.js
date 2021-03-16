import React, { useEffect, useState } from 'react';

const App = () => {
  // state hooks
  const [newTodo, setNewTodo] = useState("");
  const [todos, setTodos] = useState([]);

  // effect hook: getTodos will get called on every mount (kind of like
  // OnIntialize in Blazor, or ctor in Class component)
  useEffect(() => {
    getTodos();
  }, []);

  // GET /api/todos
  async function getTodos() {
    const result = await fetch("/api/todos");
    const todos = await result.json();
    setTodos(todos);
  }

  // POST /api/todos/{id} {newTodo-payload}
  async function createTodo(e) {
    e.preventDefault();
    await fetch("/api/todos", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        // 'Content-Type': 'application/x-www-form-urlencoded',
      },
      body: JSON.stringify({ name: newTodo }),
    });
    // clear form
    setNewTodo("");
    // refresh
    await getTodos();
  }

  // POST /api/todos/{id} (newTodo-payload)
  async function updateCompleted(todo, isComplete) {
    await fetch(`/api/todos/${todo.id}`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        // 'Content-Type': 'application/x-www-form-urlencoded',
      },
      body: JSON.stringify({ ...todo, isComplete: isComplete }),
    });
    // refresh
    await getTodos();
  }

  // DELETE /api/todos/{id}
  async function deleteTodo(id) {
    await fetch(`/api/todos/${id}`, {
      method: "DELETE",
    });
    // refresh
    await getTodos();
  }

  // view/render
  return (
    <section className="todoapp">
      <header className="header">
        <h1>todos</h1>
        <form onSubmit={createTodo}>
          <input
            className="new-todo"
            placeholder="What needs to be done?"
            value={newTodo}
            onChange={(e) => setNewTodo(e.target.value)}
          />
        </form>
      </header>
      <section className="main" style={{ display: "block" }}>
        <ul className="todo-list">
          {todos.map((todo) => {
            return (
              <li className={todo.isComplete ? "completed" : ""} key={todo.id}>
                <div className="view">
                  <input
                    className="toggle"
                    type="checkbox"
                    defaultChecked={todo.isComplete}
                    onChange={(e) => updateCompleted(todo, e.target.checked)}
                  />
                  <label>{todo.name}</label>
                  <button
                    className="destroy"
                    onClick={() => deleteTodo(todo.id)}
                  ></button>
                </div>
              </li>
            );
          })}
        </ul>
      </section>
    </section>
  );
}

export default App;