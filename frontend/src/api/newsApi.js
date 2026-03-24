import axiosClient from "./axiosClient";

// BE serialize enum thành string → gửi string trực tiếp, không convert sang số
const approvalDecisionMap = {
  Approved: 0,
  Rejected: 1,
};

const toPayload = (form) => ({
  ...form,
  // newsType giữ nguyên string vì BE expect string
});

const newsApi = {
  // ── Public ────────────────────────────────────────────
  search: (params) => axiosClient.get("/News", { params }),

  getById: (id) => axiosClient.get(`/News/${id}`),

  // === THÊM HÀM TĂNG VIEW Ở ĐÂY ===
  incrementView: (id) => axiosClient.post(`/News/${id}/view`),

  // ── Admin - Tạo/Sửa/Xóa ──────────────────────────────
  create: (data) => axiosClient.post("/News/requests/create", toPayload(data)),

  update: (id, data) =>
    axiosClient.post(
      "/News/requests/update",
      toPayload({ ...data, TargetNewsID: id }),
    ),

  delete: (id) => axiosClient.delete(`/News/${id}`),

  // ── Admin - Duyệt bài ─────────────────────────────────
  getPendingList: () => axiosClient.get("/News/requests/pending"),

  approve: (id, data) =>
    axiosClient.put(`/News/requests/${id}/approve`, {
      ...data,
      decision: approvalDecisionMap[data.decision] ?? data.decision,
    }),

  // ── Bình luận ─────────────────────────────────────────
  getComments: (newsId) => axiosClient.get(`/News/${newsId}/comments`),

  postComment: (newsId, data) =>
    axiosClient.post(`/News/${newsId}/comments`, data),

  deleteComment: (commentId) => axiosClient.delete(`/comments/${commentId}`),
};

export default newsApi;
