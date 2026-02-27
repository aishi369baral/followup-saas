const API_BASE = import.meta.env.VITE_API_BASE_URL;

export async function post(url, data) {
  const res = await fetch(`${API_BASE}${url}`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });

  if (!res.ok) throw new Error("Request failed");
  return res.json();
}